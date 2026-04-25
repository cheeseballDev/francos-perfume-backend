using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.DTOs.EmployeeDTOs;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.EmployeeServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        private readonly EmployeeCreationService employeeCreationService;
        private readonly EmployeeUpdateProfileService employeeUpdateProfileService;
        private readonly EmployeeUpdateAuthService employeeUpdateAuthService;
        public EmployeesController(DatabaseContext dbContext, IConfiguration configuration, 
            EmployeeCreationService employeeCreationService, 
            EmployeeUpdateProfileService employeeUpdateProfileService,
            EmployeeUpdateAuthService employeeUpdateAuthService)
        {
            this.dbContext = dbContext;
            this.employeeCreationService = employeeCreationService;
            this.employeeUpdateProfileService = employeeUpdateProfileService;
            this.employeeUpdateAuthService = employeeUpdateAuthService;
        }
        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayEmployees(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchId = int.Parse(claims.branchId);

            var query = dbContext.EmployeeProfiles.AsQueryable();

            if (role != "ADMIN" && role != "OWNER")
            {
                query = query.Where(i => i.branch_id == branchId);
            }

            var totalEmployees = await query.CountAsync();
            var totalEmployeesList = (int)Math.Ceiling(totalEmployees / (double)pageSize);
            var allEmployees = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var displayList = allEmployees.Select(allEmployees => new DisplayEmployeeProfileDTO
            {
                employee_id = allEmployees.employee_id,
                employee_display_id = allEmployees.employee_display_id,
                branch_id = allEmployees.branch_id,
                branch_display_id = allEmployees.branch_display_id,
                employee_full_name = allEmployees.employee_full_name,
                contact_number = allEmployees.contact_number,
                address = allEmployees.address,
                employee_shift = allEmployees.employee_shift,
                account_created = allEmployees.account_created,
                employee_profile_picture = allEmployees.employee_profile_picture
            }).ToList();

            return Ok(new
            {
                totalEmployees,
                totalEmployeesList,
                page,
                pageSize,
                data = displayList
            });
        }

        [HttpGet("displayOne/{id}")]
        public IActionResult DisplayEmployeeByID(int id)
        {
            var displayProfile = dbContext.EmployeeProfiles
                .Where(i => i.employee_id == id)
                .Select(i => new DisplayEmployeeProfileDTO
                {
                    employee_id = i.employee_id,
                    employee_display_id = i.employee_display_id,
                    branch_id = i.branch_id,
                    branch_display_id = i.branch_display_id,
                    employee_full_name = i.employee_full_name,
                    contact_number = i.contact_number,
                    address = i.address,
                    employee_shift = i.employee_shift,
                    account_created = i.account_created,
                    employee_profile_picture = i.employee_profile_picture
                })
                .FirstOrDefault();

            if (displayProfile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(displayProfile);
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var claims = new ClaimsGetter(User);
            var employeeId = Convert.ToInt32(claims.employeeId);
            var displayProfile = dbContext.EmployeeProfiles
                .Where(i => i.employee_id == employeeId)
                .Select(i => new DisplayEmployeeProfileDTO
                {
                    employee_id = i.employee_id,
                    employee_display_id = i.employee_display_id,
                    branch_id = i.branch_id,
                    branch_display_id = i.branch_display_id,
                    employee_full_name = i.employee_full_name,
                    contact_number = i.contact_number,
                    address = i.address,
                    employee_shift = i.employee_shift,
                    account_created = i.account_created,
                    employee_profile_picture = i.employee_profile_picture
                })
                .FirstOrDefault();

            if (displayProfile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(displayProfile);
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee(AddEmployeeDTO dto)
        {
            var claims = new ClaimsGetter(User);

            var result = await employeeCreationService.AddProfileAsync(dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);

        }

        [HttpPut("updateProfile/{id:int}")]
        public async Task<IActionResult> UpdateEmployeeProfile(int id, UpdateEmployeeProfileDTO dto)
        {
            var claims = new ClaimsGetter(User);

            var result = await employeeUpdateProfileService.UpdateProfileAsync(id, dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("updateAuth/{id:int}")]
        public async Task<IActionResult> UpdateEmployeeAuth(int id, UpdateEmployeeAuthDTO update)
        {
            var claims = new ClaimsGetter(User);

            var result = await employeeUpdateAuthService.UpdateEmployeeAuthAsync(id, update, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
