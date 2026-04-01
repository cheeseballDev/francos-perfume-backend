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
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseContext dbContext;

        public ProductsController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("displayAll")]
        public IActionResult DisplayProducts()
        {
            var allProducts = dbContext.Products.ToList();
            return Ok(allProducts);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProducts(AddProductDTO dto)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var product = new Products
                {
                    product_name = dto.product_name,
                    product_type = dto.product_type,
                    product_note = dto.product_note,
                    product_gender = dto.product_gender,
                    product_barcode = dto.product_barcode,
                    product_date_created = DateTime.UtcNow,
                    product_status = "Active"
                };

                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    product.product_id,
                    product.product_display_id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    SELECT setval(
                        'productstable_product_id_seq',
                        (SELECT MAX(product_id) FROM productstable)
                    );
                ");
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
