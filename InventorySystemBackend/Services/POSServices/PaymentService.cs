using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.POSDTOs;
using InventorySystemBackend.Models.Entities;

namespace InventorySystemBackend.Services
{
    public class PaymentService
    {
        private readonly DatabaseContext dbContext;

        public PaymentService(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Payment> CreatePayment(int orderId, POSDTO dto, decimal total)
        {
            var payment = new Payment
            {
                sales_order_id = orderId,
                payment_method = dto.payment_method,
                amount_paid = dto.amount_paid,
                change_amount = dto.amount_paid - total,
                payment_date = DateTime.UtcNow
            };

            dbContext.Payments.Add(payment);
            return payment;
        }
    }
}
