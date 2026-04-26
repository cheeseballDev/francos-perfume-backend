using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.RequestDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Services.RequestServices
{
    public class CreateRequestService
    {
        private readonly DatabaseContext dbContext;
        public CreateRequestService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ServiceResult> CreateRequestAsync(CreateRequestDTO dto, ClaimsGetter claims)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userId = claims.employeeId;
                var userBranchId = int.Parse(claims.branchId);
                var userBranch = claims.branchDisplayId;

                var exists = await dbContext.Products
                    .AnyAsync(p => p.product_id == dto.product_id);

                if (!exists)
                    return ServiceResult.Fail("Product does not exist");

                var fromBranch = await dbContext.Branches
                    .FirstOrDefaultAsync(b => b.branch_id == userBranchId);

                if (fromBranch == null)
                    return ServiceResult.Fail("User branch not found");

                var toBranch = await dbContext.Branches
                    .FirstOrDefaultAsync(b => b.branch_id == dto.delivered_to_branch_id);

                if (toBranch == null)
                    return ServiceResult.Fail("Destination branch not found");

                var request = new Requests
                {
                    product_id = dto.product_id,
                    branch_id = fromBranch.branch_id,
                    delivered_to_branch_id = dto.delivered_to_branch_id,
                    employee_id = int.Parse(userId),
                    request_qty = dto.request_qty,
                    request_date_submitted = DateTime.UtcNow,
                    request_status = "active",
                    request_message = dto.request_message,
                    requested_from = fromBranch.branch_location,
                    delivered_to = toBranch.branch_location,
                    delivery_type = dto.delivery_type
                };

                dbContext.Requests.Add(request);
                await dbContext.SaveChangesAsync();

                var audit = new AuditLogs
                {
                    employee_display_id = user,
                    branch_display_id = userBranch,
                    log_action = $"Sent Request ({request.request_display_id})",
                    log_module = "Product Management",
                    log_timestamp = DateTime.UtcNow
                };
                dbContext.AuditLogs.Add(audit);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return ServiceResult.Ok(new
                {
                    request.request_id,
                    request.request_display_id
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
