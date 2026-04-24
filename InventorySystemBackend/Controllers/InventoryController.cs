using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.DTOs.InventoryDTOs;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.ProductServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        private readonly ProductAddingService productAddingService;

        public InventoryController(DatabaseContext dbContext, ProductAddingService productAddingService)
        {
            this.dbContext = dbContext;
            this.productAddingService = productAddingService;
        }

        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayInventory()
        {

            var branch_id = int.Parse(User.Claims
                    .First(c => c.Type == "branch_id").Value);

            var inventory = dbContext.Inventories
                .Select(i => new InventoryDisplayDTO
                {
                    product_id = i.product_id,
                    //branch_display_id = i.Branch.branch_display_id,
                    branch_name = i.Branch.branch_location,
                    product_qty = i.product_qty,
                    product_display_id = i.Products.product_display_id,
                    product_name = i.Products.product_name,
                    product_type = i.Products.product_type,
                    product_note = i.Products.product_note,
                    product_gender = i.Products.product_gender,
                    product_barcode = i.Products.product_barcode,
                    product_status = i.Products.product_status,
                    product_price = i.Products.product_price,
                    product_image_url = i.Products.product_image_url,
                    product_date_created = i.Products.product_date_created
                })
                .ToList();

            if (!inventory.Any())
                return NotFound("No inventory found.");

            return Ok(inventory);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddInventory(AddProductDTO dto) //Change to AddInventoryDTO if you want to add inventory without creating a new product. For now, this will create a new product and add it to the inventory of the user's branch.
        {//tangina angas ng autofill lmao
            var claims = new ClaimsGetter(User);

            var result = await productAddingService.AddProductAsync(dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
            /* //FOR CENTRALIZED INVENTORY MANAGEMENT
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try {
                var claims = new ClaimsGetter(User);
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;
                var branch_id = int.Parse(claims.branchId);
                    

                var existingInventory = await dbContext.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.product_id == dto.product_id &&
                        i.branch_id == branch_id);


                var product = await dbContext.Products
                    .FirstOrDefaultAsync(p => p.product_id == dto.product_id);

                if (product == null)
                    return BadRequest("Product not found.");

                if (product.product_status != "active")
                    return BadRequest("Product is arhived.");

                if(dto.product_quantity <= 0)
                    return BadRequest("Product cannot be set to zero or negative.");

                if (existingInventory != null)
                {
                    existingInventory.product_qty += dto.product_quantity;
                }
                else
                {
                    dbContext.Inventories.Add(new Inventory
                    {
                        product_id = dto.product_id,
                        branch_id = branch_id,
                        product_qty = dto.product_quantity
                    });
                }

                await dbContext.SaveChangesAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Added to inventory ({product.product_display_id})",
                    log_module = "Employee Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("Inventory updated.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.ToString());
            }
            */
        }

        [HttpPatch("updateQuantity/{id:int}")]
        public async Task<IActionResult> UpdateQuantity(int id, int qty)
        {
            var claims = new ClaimsGetter(User);
            var branch_id = int.Parse(claims.branchId);

            var existingInventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i =>
                    i.product_id == id &&
                    i.branch_id == branch_id);

            if (existingInventory == null)
                return NotFound();

            var updatedQty = existingInventory.product_qty + qty;

            if (updatedQty < 0)
                return BadRequest("Product cannot be negative.");

            existingInventory.product_qty = updatedQty;

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                existingInventory.product_id,
                existingInventory.product_qty
            });
        }
    }
}