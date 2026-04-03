using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var empId = User.GetEmployeeId();
            var profile = dbContext.EmployeeProfiles.FirstOrDefault(x => x.employee_id.ToString() == empId);
            return Ok(new { profile });
        }

        [HttpGet("displayAllAuths")]//TESTING LANG DAHIL TINATAMAD AKO MAG ALT TAB SA PGADMIN4 BURAHIN BAGO IFULL RELEASE
        public IActionResult DisplayEmployeesAuth()
        {
            var allEmployeesAuth = dbContext.EmployeeAuths.ToList();
            return Ok(allEmployeesAuth);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee(AddEmployeeDTO dto)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                if (await dbContext.EmployeeAuths.AnyAsync(x => x.email == dto.email))
                    return BadRequest("Email already exists");

                var employee = new EmployeeProfiles
                {
                    branch_id = dto.branch_id,
                    employee_full_name = dto.full_name,
                    contact_number = dto.contact_number,
                    address = dto.address,
                    employee_shift = dto.employee_shift,
                    account_created = DateTime.UtcNow,
                    employee_profile_picture = dto.employee_profile_picture
                };

                dbContext.EmployeeProfiles.Add(employee);
                await dbContext.SaveChangesAsync();

                var tempPassword = "PUTANGINAMO";
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

                await transaction.CommitAsync();

                return Ok(new
                {
                    employee.employee_id,
                    employee.employee_display_id,
                    employee.employee_full_name
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

        [HttpPut("updateProfile/{id:int}")]
        public async Task<IActionResult> UpdateEmployeeProfile(int id, UpdateEmployeeProfileDTO update)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employee = await dbContext.EmployeeProfiles.FindAsync(id);

                if (employee == null)
                    return NotFound();

                employee.branch_id = update.branch_id;
                employee.contact_number = update.contact_number;
                employee.address = update.address;
                employee.employee_full_name = update.full_name;
                employee.employee_shift = update.employee_shift;
                employee.employee_profile_picture = update.employee_profile_picture;

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    employee.employee_id,
                    employee.employee_full_name,
                });
            }
            catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ex.ToString());
                }
            }

        [HttpPut("updateAuth/{id:int}")]
        public async Task<IActionResult> UpdateEmployeeAuth(int id, UpdateEmployeeAuthDTO update)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employeeAuth = await dbContext.EmployeeAuths.FindAsync(id);

                if (employeeAuth == null)
                    return NotFound();

                var auth = await dbContext.EmployeeAuths
                    .FirstOrDefaultAsync(x => x.employee_id == id);

                if (auth == null)
                    return NotFound("Auth record not found");

                auth.email = update.email;
                auth.employee_role = update.employee_role;
                auth.password_status = update.password_status;

                if (auth.password_status == "active" && !string.IsNullOrEmpty(update.password))
                {
                    var hasher = new PasswordHasher<object>();
                    auth.password_hash = hasher.HashPassword(null, update.password);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    auth.email,
                    auth.employee_role,
                    auth.password_hash, //TESTING LANG DIN BURAHIN BAGO IFULL RELEASE
                    auth.password_status //ISA PATO
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
