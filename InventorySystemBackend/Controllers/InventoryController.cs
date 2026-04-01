using InventorySystemBackend.Data;
using InventorySystemBackend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystemBackend.DTOs;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly DatabaseContext dbContext;

        public InventoryController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayInventory(int branchId)
        {
            var inventory = await dbContext.Inventories
                .Where(i => i.branch_id == branchId && i.product_qty > 0 && i.Products.product_status == "Active")
                .Select(i => new InventoryDisplayDTO
                {
                    ProductId = i.product_id,
                    ProductDisplayId = i.Products.product_display_id,
                    ProductName = i.Products.product_name,
                    ProductType = i.Products.product_type,
                    ProductStatus = i.Products.product_status,
                    Quantity = i.product_qty
                })
                .ToListAsync();

            if (!inventory.Any())
                return NotFound("No inventory found.");

            return Ok(inventory);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddInventory(AddInventoryDTO dto)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try {
                /*
                var branch_id = int.Parse(User.Claims
                    .First(c => c.Type == "branch_id").Value);
                    */ //para to sa final product, tangina diko mapagana ng ganto lang sa swagger + walang login function


                var existingInventory = await dbContext.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.product_id == dto.product_id &&
                        i.branch_id == dto.branch_id);


                var product = await dbContext.Products
                    .FirstOrDefaultAsync(p => p.product_id == dto.product_id);

                if (product == null || product.product_status != "Active")
                {
                    return BadRequest("Product is not active.");
                }

                if (existingInventory != null)
                {
                    existingInventory.product_qty += dto.product_quantity;
                }
                else
                {
                    dbContext.Inventories.Add(new Inventory
                    {
                        product_id = dto.product_id,
                        branch_id = dto.branch_id,
                        product_qty = dto.product_quantity
                    });
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Inventory updated.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.ToString());
            }
        }
    }
}