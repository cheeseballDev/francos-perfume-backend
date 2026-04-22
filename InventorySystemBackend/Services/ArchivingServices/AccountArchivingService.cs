using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventorySystemBackend.Services.ArchivingServices
{
    public class AccountArchivingService
    {
        private readonly DatabaseContext dbContext;
        public AccountArchivingService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> ArchiveAccount(int id, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employeeId = Convert.ToInt32(claims.employeeDisplayId);
                var profile = dbContext.EmployeeProfiles.FirstOrDefault(x => x.employee_id == employeeId);

                var auth = await dbContext.EmployeeAuths
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.employee_id == id);

                if (auth == null)
                    return ServiceResult.Fail("Auth record not found");

                if (auth.account_status == "archived")
                    return ServiceResult.Fail("Account is already archived");

                var archivedBy = profile.employee_display_id ?? "Jeffrey Epstein";

                var archive = new ArchivedAccounts
                {
                    employee_display_id = auth.Employee.employee_display_id,
                    email = auth.email,
                    employee_role = auth.employee_role,
                    branch_id = auth.Employee.branch_id,
                    archived_by = archivedBy,
                    date_archived = DateTime.UtcNow
                };
                dbContext.ArchivedAccounts.Add(archive);

                auth.account_status = "archived";

                var audit = new AuditLogs
                {
                    employee_display_id = profile.employee_display_id,
                    branch_display_id = profile.branch_display_id,
                    log_action = $"Archived account ({auth.Employee.employee_display_id})",
                    log_module = "Employee Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    archive.account_archive_id,
                    archive.account_archive_display_id,
                    auth.employee_id,
                    auth.account_status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail(ex.ToString());
            }
        }
    }
}
