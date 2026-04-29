using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.EmployeeDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace InventorySystemBackend.Services.EmployeeServices
{
    public class EmployeeCreationService
    {
        private readonly DatabaseContext dbContext;
        private readonly IConfiguration configuration;

        public EmployeeCreationService(DatabaseContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        public async Task<ServiceResult> AddProfileAsync(AddEmployeeDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;


                if (await dbContext.EmployeeAuths.AnyAsync(x => x.email == dto.email))
                    return ServiceResult.Fail("Email already exists");

                var branch = await dbContext.Branches
                    .FirstOrDefaultAsync(b => b.branch_id == dto.branch_id);

                if (branch == null)
                {
                    return ServiceResult.Fail("Branch not found");
                }

                var employee = new EmployeeProfiles
                {
                    branch_id = dto.branch_id,
                    branch_display_id = branch.branch_display_id,
                    employee_full_name = (dto.first_name + " " + dto.middle_name + " " + dto.last_name),
                    contact_number = dto.contact_number,
                    address = dto.address,
                    employee_shift = dto.employee_shift,
                    account_created = DateTime.UtcNow,
                    employee_profile_picture = dto.employee_profile_picture
                };

                dbContext.EmployeeProfiles.Add(employee);
                await dbContext.SaveChangesAsync();

                var tempPassword = configuration["DefaultPassword:Password"];
                var hasher = new PasswordHasher<object>();
                var hashedPassword = hasher.HashPassword(null, tempPassword);

                var auth = new EmployeeAuths
                {
                    employee_id = employee.employee_id,
                    email = dto.email,
                    password_hash = hashedPassword,
                    password_status = "temporary",
                    auth_provider = "local",
                    employee_role = dto.employee_role,
                    account_status = "active",
                    created_at = DateTime.UtcNow
                };

                dbContext.EmployeeAuths.Add(auth);
                await dbContext.SaveChangesAsync();

                var newEmployee = employee.employee_display_id;
                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Added account ({newEmployee})",
                    log_module = "Employee Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    employee.employee_id,
                    employee.employee_display_id,
                    employee.employee_full_name
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
