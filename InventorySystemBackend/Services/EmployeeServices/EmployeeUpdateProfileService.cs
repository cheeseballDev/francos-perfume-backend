using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.EmployeeDTOs;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Services.EmployeeServices
{
    public class EmployeeUpdateProfileService
    {
        private readonly DatabaseContext dbContext;
        public EmployeeUpdateProfileService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<ServiceResult> UpdateProfileAsync(int id, UpdateEmployeeProfileDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employee = await dbContext.EmployeeProfiles.FindAsync(id);

                if (employee == null)
                    return ServiceResult.Fail("Employee not found");

                var branch = await dbContext.Branches
                    .FirstOrDefaultAsync(b => b.branch_id == dto.branch_id);

                if (branch == null)
                {
                    return ServiceResult.Fail("Branch not found");
                }

                employee.branch_id = dto.branch_id;
                employee.branch_display_id = branch.branch_display_id;
                employee.contact_number = dto.contact_number;
                employee.address = dto.address;
                employee.employee_full_name = (dto.first_name + " " + dto.middle_name + " " + dto.last_name);
                employee.employee_shift = dto.employee_shift;
                employee.employee_profile_picture = dto.employee_profile_picture;

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    employee.employee_display_id,
                    employee.employee_full_name,
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
