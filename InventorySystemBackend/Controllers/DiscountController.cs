using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly DatabaseContext dbContext;

        public DiscountController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("displayAllDiscounts")]
        public async Task<IActionResult> GetDiscounts()
        {
            var discount = await dbContext.Discounts.ToListAsync();
            var displayDiscounts = new List<DiscountsDTO>();
            foreach (var discounts in displayDiscounts)
            {
                displayDiscounts.Add(new DiscountsDTO
                {
                    discount_name = discounts.discount_name,
                    discount_percent = discounts.discount_percent,
                    discount_amount = discounts.discount_amount,
                    discount_status = discounts.discount_status,
                    discount_prefix = discounts.discount_prefix
                });
            }
            return Ok(displayDiscounts);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddDiscount(DiscountsDTO dto)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var claims = new ClaimsGetter(User);
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;
                var branch_id = int.Parse(claims.branchId);

                if (dto.discount_amount < 0 || dto.discount_percent < 0)
                    return BadRequest("Discount cannot be set to negative.");

                if(dto.discount_percent > 100)
                    return BadRequest("Discount percent cannot be greater than 100.");

                var percent = dto.discount_percent / 100;
                var discount = new Discounts
                {
                    discount_name = dto.discount_name,
                    discount_amount = dto.discount_amount,
                    discount_percent = percent,
                    discount_status = dto.discount_status,
                    discount_created = DateTime.UtcNow,
                    discount_prefix = dto.discount_prefix
                };
                dbContext.Discounts.Add(discount);
                await dbContext.SaveChangesAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Added a new discount ({dto.discount_name})",
                    log_module = "Discount Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("Discount added.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.ToString());
            }
        }
    }
}