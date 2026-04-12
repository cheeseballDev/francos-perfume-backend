using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POSController : ControllerBase
    {
        private readonly DatabaseContext dbContext;

        public POSController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] POSDTO dto)//WIP, BASIC FUNCTIONS
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var claims = new ClaimsGetter(User);
                var employeeId = Convert.ToInt32(claims.employeeId);
                var branchId = Convert.ToInt32(claims.branchId);
                var role = claims.role;
                var user = claims.employeeDisplayId;
                var userBranch = claims.GetBranchDisplayId(User);

                var order = new SalesOrder
                {
                    employee_id = employeeId,
                    branch_id = branchId,
                    sales_timestamp = DateTime.UtcNow,
                    total_amount = 0
                };

                dbContext.SalesOrders.Add(order);
                await dbContext.SaveChangesAsync();

                decimal total = 0;

                foreach (var item in dto.items)
                {
                    var productId = item.product_id;
                    var quantity = item.quantity;

                    var product = await dbContext.Products
                        .FirstOrDefaultAsync(p => p.product_id == productId);

                    if (product == null)
                        throw new Exception("Product not found");

                    var unitPrice = product.product_price;

                    var detail = new SalesOrderDetail
                    {
                        sales_order_id = order.sales_order_id,
                        product_id = productId,
                        quantity = quantity,
                        unit_price = unitPrice
                    };

                    dbContext.SalesOrderDetails.Add(detail);
                    await ReduceStock(item.product_id, branchId, item.quantity);
                }

                order.total_amount = total;

                if(dto.amount_paid < total)
                    throw new Exception("Amount paid is less than total amount.");

                var payment = new Payment
                {
                    sales_order_id = order.sales_order_id,
                    payment_method = dto.payment_method,
                    amount_paid = dto.amount_paid,
                    payment_date = DateTime.UtcNow
                };

                dbContext.Payments.Add(payment);
                dbContext.SaveChanges();

                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(order.sales_order_id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();           
                return StatusCode(500, ex.ToString());
            }
        }

        private async Task ReduceStock(int productId, int branchId, int qty)
        {
            var inventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i =>
                    i.product_id == productId &&
                    i.branch_id == branchId);

            if (inventory == null)
                throw new Exception("Inventory not found");

            inventory.product_qty -= qty;

            if (inventory.product_qty < 0)
                throw new Exception("Insufficient stock");
        }
    }
}
