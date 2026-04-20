using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.EmployeeDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace InventorySystemBackend.Services.EmployeeServices
{
    public class EmployeeUpdateAuthService
    {
        private readonly DatabaseContext dbContext;
        public EmployeeUpdateAuthService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> UpdateEmployeeAuthAsync(int id, UpdateEmployeeAuthDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;

                var employeeAuth = await dbContext.EmployeeAuths.FindAsync(id);

                if (employeeAuth == null)
                    return ServiceResult.Fail("Employee auth not found");

                var auth = await dbContext.EmployeeAuths
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(x => x.employee_id == id);

                if (auth == null)
                    return ServiceResult.Fail("Auth record not found");

                auth.email = dto.email;
                auth.employee_role = dto.employee_role;
                auth.password_status = dto.password_status;

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Updated account authentication ({auth.Employee.employee_display_id})",
                    log_module = "Employee Management",
                    log_timestamp = DateTime.UtcNow
                };

                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                return ServiceResult.Ok(new
                {
                    auth.email,
                    auth.employee_role,
                    auth.password_status
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
