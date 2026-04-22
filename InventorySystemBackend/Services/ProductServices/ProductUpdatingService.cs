using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace InventorySystemBackend.Services.ProductServices
{
    public class ProductUpdatingService
    {
        private readonly DatabaseContext dbContext;

        public ProductUpdatingService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> UpdateProductAsync(int id, UpdateProductDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;
                var products = await dbContext.Products.FindAsync(id);

                if (products == null)
                    return ServiceResult.Fail("Product not found");

                products.product_name = dto.product_name;
                products.product_type = dto.product_type;
                products.product_note = dto.product_note;
                products.product_gender = dto.product_gender;
                products.product_barcode = dto.product_barcode;
                products.product_description = dto.product_description;
                products.product_price = dto.product_price;
                products.product_image_url = dto.product_image_url;

                await dbContext.SaveChangesAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Updated product ({products.product_display_id})",
                    log_module = "Product Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    products.product_display_id,
                    products.product_name,
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail(ex.ToString());
            }
        }
    }
}
