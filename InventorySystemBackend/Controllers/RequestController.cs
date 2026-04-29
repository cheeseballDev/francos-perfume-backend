using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.InventoryDTOs;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.DTOs.RequestDTOs;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.ProductServices;
using InventorySystemBackend.Services.RequestServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    public class RequestController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        private readonly CreateRequestService createRequestService;

        public RequestController(DatabaseContext dbContext, CreateRequestService createRequestService)
        {
            this.dbContext = dbContext;
            this.createRequestService = createRequestService;
        }

        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayRequests(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var userBranchDisplayId = claims.branchDisplayId;

            var totalRequests = await dbContext.Requests.CountAsync();
            var totalRequestsPages = (int)Math.Ceiling(totalRequests / (double)pageSize);
            var allRequests = await dbContext.Requests
                .Where(r => r.requested_from == userBranchDisplayId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var displayList = allRequests.Select(allRequests => new DisplayRequestDTO
            {
                request_display_id = allRequests.request_display_id,
                product_name = allRequests.Product.product_name,
                employee_display_id = allRequests.Employee.employee_display_id,
                request_qty = allRequests.request_qty,
                request_date_submitted = allRequests.request_date_submitted,
                request_message = allRequests.request_message,
                request_status = allRequests.request_status,
                requested_from = allRequests.FromBranch.branch_location,
                delivered_to = allRequests.ToBranch.branch_location,
                delivery_type = allRequests.delivery_type
            }).ToList();

            return Ok(new
            {
                totalRequests,
                totalRequestsPages,
                page,
                pageSize,
                data = displayList
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest(CreateRequestDTO dto)
        {
            var claims = new ClaimsGetter(User);

            var result = await createRequestService.CreateRequestAsync(dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("displayOneRequest/{id:int}")]
        public async Task<IActionResult> RequestDetails(int id)
        {
            var displayRequest = dbContext.Requests
                .Where(i => i.request_id == id)
                .Select(i => new DisplayRequestDTO            
                {
                    request_display_id = i.request_display_id,
                    product_name = i.Product.product_name,
                    employee_display_id = i.Employee.employee_display_id,
                    request_qty = i.request_qty,
                    request_date_submitted = i.request_date_submitted,
                    request_message = i.request_message,
                    request_status = i.request_status,
                    requested_from = i.FromBranch.branch_location,
                    delivered_to = i.ToBranch.branch_location,
                    delivery_type = i.delivery_type
                })
                .FirstOrDefault();

            if (displayRequest == null)
            {
                return NotFound(new { message = "Request not found" });
            }

            return Ok(displayRequest);
        }
    }
}
