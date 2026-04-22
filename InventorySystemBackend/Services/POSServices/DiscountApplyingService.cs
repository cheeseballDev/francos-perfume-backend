using InventorySystemBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Services
{
    public class DiscountApplyingService
    {
        private readonly DatabaseContext dbContext;

        public DiscountApplyingService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<decimal> ApplyDiscount(int? discountId, decimal total)
        {
            if (discountId == null) return total;

            var discount = await dbContext.Discounts
                .FirstOrDefaultAsync(d => d.discount_id == discountId);

            if (discount == null || discount.discount_status == "inactive")
                return total;

            var percent = discount.discount_percent == 0 ? 1 : discount.discount_percent;
            var fixedAmount = discount.discount_amount;

            return Math.Max(0, (total * percent) - fixedAmount);
        }
    }
}
