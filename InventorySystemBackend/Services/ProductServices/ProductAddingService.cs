using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.ProductDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace InventorySystemBackend.Services.ProductServices
{
    public class ProductAddingService
    {
        private readonly DatabaseContext dbContext;

        public ProductAddingService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> AddProductAsync(AddProductDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.branchDisplayId;

                var products = new Products
                {
                    product_name = dto.product_name,
                    product_type = dto.product_type,
                    product_note = dto.product_note,
                    product_gender = dto.product_gender,
                    product_barcode = dto.product_barcode,
                    product_date_created = DateTime.UtcNow,
                    product_status = "active",
                    product_description = dto.product_description,
                    product_price = dto.product_price,
                    product_image_url = dto.product_image_url
                };

                dbContext.Products.Add(products);
                await dbContext.SaveChangesAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Added product ({products.product_display_id})",
                    log_module = "Product Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    products.product_id,
                    products.product_display_id
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
