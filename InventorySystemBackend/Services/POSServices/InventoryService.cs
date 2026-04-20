using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.POSDTOs;
using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Services
{
    public class InventoryService
    {
        private readonly DatabaseContext dbContext;

        public InventoryService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<(decimal, List<SalesOrderDetail>, string?)> ProcessItems(List<SalesOrderItemDTO> items, int branchId, int orderId)
        {
            var productIds = items.Select(i => i.product_id).ToList();

            var products = await dbContext.Products
                .Where(p => productIds.Contains(p.product_id))
                .ToDictionaryAsync(p => p.product_id);

            var inventories = await dbContext.Inventories
                .Where(i => productIds.Contains(i.product_id) && i.branch_id == branchId)
                .ToDictionaryAsync(i => i.product_id);

            decimal total = 0;
            var details = new List<SalesOrderDetail>();

            foreach (var item in items)
            {
                if (!products.ContainsKey(item.product_id))
                    return (0, null, "Product does not exist");

                if (!inventories.ContainsKey(item.product_id))
                    return (0, null, "Item not in inventory");

                var product = products[item.product_id];
                var inventory = inventories[item.product_id];

                if (inventory.product_qty < item.quantity)
                    return (0, null, "Insufficient stock");

                inventory.product_qty -= item.quantity;

                var lineTotal = product.product_price * item.quantity;
                total += lineTotal;

                details.Add(new SalesOrderDetail
                {
                    sales_order_id = orderId,
                    product_id = item.product_id,
                    quantity = item.quantity,
                    unit_price = product.product_price
                });
            }

            return (total, details, null);
        }
    }
}
