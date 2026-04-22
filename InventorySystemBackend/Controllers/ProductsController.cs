using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.EmployeeServices;
using InventorySystemBackend.Services.ProductServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        private readonly ProductAddingService productAddingService;
        private readonly ProductUpdatingService productUpdatingService;

        public ProductsController(DatabaseContext dbContext, ProductAddingService productAddingService, ProductUpdatingService productUpdatingService)
        {
            this.dbContext = dbContext;
            this.productAddingService = productAddingService;
            this.productUpdatingService = productUpdatingService;
        }

        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayProducts()
        {
            var products = await dbContext.Products.ToListAsync();
            var productList = new List<DisplayProductDTO>();
            foreach (var product in products)
            {
                productList.Add(new DisplayProductDTO
                {
                    product_display_id = product.product_display_id,
                    product_name = product.product_name,
                    product_type = product.product_type,
                    product_note = product.product_note,
                    product_gender = product.product_gender,
                    product_barcode = product.product_barcode,
                    product_date_created = product.product_date_created,
                    product_description = product.product_description,
                    product_price = product.product_price,
                    product_image_url = product.product_image_url
                });
            }
            return Ok(productList);
        }

        [HttpGet("displayOne/{id}")]
        public IActionResult DisplayProductByID(int id)
        {
            var product = dbContext.Products.FirstOrDefault(p => p.product_id == id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProducts(AddProductDTO dto)
        {
            var claims = new ClaimsGetter(User);

            var result = await productAddingService.AddProductAsync(dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("updateProduct/{id:int}")]
        public async Task<IActionResult> UpdateProductInformation(int id, UpdateProductDTO dto)
        {
            var claims = new ClaimsGetter(User);

            var result = await productUpdatingService.UpdateProductAsync(id, dto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }       
    }
}
