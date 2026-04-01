using InventorySystemBackend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        public EmployeesController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet("displayAll")]
        public IActionResult DisplayEmployees()
        {
            var allEmployees = dbContext.EmployeeProfiles.ToList();
            return Ok(allEmployees);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee(AddEmployeeDTO dto)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employee = new EmployeeProfile
                {
                    branch_id = dto.branch_id,
                    full_name = dto.full_name,
                    contact_number = dto.contact_number,
                    address = dto.address,
                    employee_shift = dto.employee_shift,
                    account_created = DateTime.UtcNow,
                    account_status = "Active",
                    employee_profile_picture = dto.employee_profile_picture
                };

                dbContext.EmployeeProfiles.Add(employee);
                await dbContext.SaveChangesAsync();

                var auth = new EmployeeAuth
                {
                    employee_id = employee.employee_id,
                    email = dto.email,
                    password_hash = dto.password ?? "TEMP",
                    password_status = "temporary",
                    auth_provider = "local",
                    employee_role = dto.employee_role,
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                dbContext.EmployeeAuths.Add(auth);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    employee.employee_id,
                    employee.employee_display_id,
                    employee.full_name
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await dbContext.Database.ExecuteSqlRawAsync(@"
                    SELECT setval(
                    'employeeprofiletable_employee_id_seq',
                    (SELECT MAX(employee_id) FROM employeeprofiletable)
                    );");

                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDTO update)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employee = await dbContext.EmployeeProfiles.FindAsync(id);

                if (employee == null)
                    return NotFound();

                var auth = await dbContext.EmployeeAuths
                    .FirstOrDefaultAsync(x => x.employee_id == id);

                if (auth == null)
                    return NotFound("Auth record not found");

                employee.branch_id = update.branch_id;
                employee.contact_number = update.contact_number;
                employee.address = update.address;
                employee.full_name = update.full_name;
                employee.employee_shift = update.employee_shift;
                employee.employee_profile_picture = update.employee_profile_picture;

                auth.email = update.email;
                auth.employee_role = update.employee_role;

                if (!string.IsNullOrEmpty(update.password))
                {
                    auth.password_hash = update.password;
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    employee.employee_id,
                    employee.full_name,
                    auth.email,
                    auth.employee_role
                });
            }
            catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ex.ToString());
                }
            }
        }
    }
