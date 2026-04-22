using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventorySystemBackend.Services.ArchivingServices
{
    public class ProductArchivingService        
    {
        private readonly DatabaseContext dbContext;
        public ProductArchivingService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> ArchiveProduct(int id, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var employeeId = Convert.ToInt32(claims.employeeDisplayId);
                var profile = dbContext.EmployeeProfiles.FirstOrDefault(x => x.employee_id == employeeId);

                var product = await dbContext.Products
                    .FirstOrDefaultAsync(x => x.product_id == id);

                if (product == null)
                    return ServiceResult.Fail("Product not found");

                if (product.product_status == "archived")
                    return ServiceResult.Fail("Product is already archived");

                var archivedBy = profile.employee_display_id ?? "Jeffrey Epstein";

                var archive = new ArchivedProducts
                {
                    product_display_id = product.product_display_id,
                    product_name = product.product_name,
                    product_type = product.product_type,
                    product_note = product.product_note,
                    product_gender = product.product_gender,
                    product_barcode = product.product_barcode,
                    archived_by = archivedBy,
                    date_archived = DateTime.UtcNow
                };
                dbContext.ArchivedProducts.Add(archive);

                product.product_status = "archived";

                var audit = new AuditLogs
                {
                    employee_display_id = profile.employee_display_id,
                    branch_display_id = profile.branch_display_id,
                    log_action = $"Archived product ({product.product_display_id})",
                    log_module = "Product Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    archive.product_archive_id,
                    archive.product_archive_display_id,
                    product.product_id,
                    product.product_status
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