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
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> DisplayInventory(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var branch_id = int.Parse(claims.branchId);
            var branchId = int.Parse(claims.branchId);
            var role = claims.role;

            var query = dbContext.Inventories
                .Include(i => i.Branch)
                .Include(i => i.Products)
                .AsQueryable();

            if (role != "OWNER")
            {
                query = query.Where(i => i.branch_id == branchId);
            }

            var totalInventories = await query.CountAsync();
            var totalInventoriesPages = (int)Math.Ceiling(totalInventories / (double)pageSize);
            var allInventories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var displayList = allInventories.Select(allInventories => new InventoryDisplayDTO
            {
                product_id = allInventories.product_id,
                branch_display_id = allInventories.Branch.branch_display_id,
                branch_name = allInventories.Branch.branch_location,
                product_qty = allInventories.product_qty,
                product_display_id = allInventories.Products.product_display_id,
                product_name = allInventories.Products.product_name,
                product_type = allInventories.Products.product_type,
                product_note = allInventories.Products.product_note,
                product_gender = allInventories.Products.product_gender,
                product_barcode = allInventories.Products.product_barcode,
                product_status = allInventories.Products.product_status,
                product_price = allInventories.Products.product_price,
                product_image_url = allInventories.Products.product_image_url,
                product_date_created = allInventories.Products.product_date_created
            }).ToList();

            return Ok(new
            {
                totalInventories,
                totalInventoriesPages,
                page,
                pageSize,
                data = displayList
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddInventory(UpdateQuantityDTO dto)
        {
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
                    log_module = "Inventory Management",
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