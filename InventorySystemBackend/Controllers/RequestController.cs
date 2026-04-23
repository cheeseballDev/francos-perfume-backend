using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.RequestDTOs;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.ProductServices;
using InventorySystemBackend.Services.RequestServices;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> DisplayRequests()
        {
            var requestList = await dbContext.Requests
                .Select(r => new DisplayRequestDTO
                {
                    request_display_id = r.request_display_id,
                    product_name = r.Product.product_name,
                    employee_display_id = r.Employee.employee_display_id,
                    request_qty = r.request_qty,
                    request_date_submitted = r.request_date_submitted,
                    request_message = r.request_message,
                    request_status = r.request_status,
                    requested_from = r.FromBranch.branch_location,
                    delivered_to = r.ToBranch.branch_location,
                    delivery_type = r.delivery_type
                })
                .ToListAsync();

            return Ok(requestList);
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
    }
}
