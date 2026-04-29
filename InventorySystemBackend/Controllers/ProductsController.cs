using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.DTOs.InventoryDTOs;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.EmployeeServices;
using InventorySystemBackend.Services.ProductServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public async Task<IActionResult> DisplayProducts(int page = 1, int pageSize = 20)
        {
            var totalProducts = await dbContext.Products.CountAsync();
            var totalProductsPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var allProducts = await dbContext.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var displayList = allProducts.Select(allProducts => new DisplayProductDTO
            {
                product_display_id = allProducts.product_display_id,
                product_name = allProducts.product_name,
                product_type = allProducts.product_type,
                product_note = allProducts.product_note,
                product_gender = allProducts.product_gender,
                product_barcode = allProducts.product_barcode,
                product_date_created = allProducts.product_date_created,
                product_description = allProducts.product_description,
                product_price = allProducts.product_price,
                product_image_url = allProducts.product_image_url
            }).ToList();

            return Ok(new
            {
                totalProducts,
                totalProductsPages,
                page,
                pageSize,
                data = displayList
            });
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

        [HttpPut("displayOneProduct/{id:int}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var displayProduct = dbContext.Products
                .Where(i => i.product_id == id)
                .Select(i => new DisplayProductDTO
                {
                    product_display_id = i.product_display_id,
                    product_name = i.product_name,
                    product_type = i.product_type,
                    product_note = i.product_note,
                    product_gender = i.product_gender,
                    product_barcode = i.product_barcode,
                    product_date_created = i.product_date_created,
                    product_description = i.product_description,
                    product_price = i.product_price,
                    product_image_url = i.product_image_url
                })
                .FirstOrDefault();

            if (displayProduct == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(displayProduct);
        }
    }
}
