using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.POSDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;

public class POSService
{
    private readonly DatabaseContext dbContext;
    private readonly InventoryService inventoryService;
    private readonly DiscountApplyingService discountService;
    private readonly PaymentService paymentService;

    public POSService(DatabaseContext dbContext, InventoryService inventoryService, DiscountApplyingService discountService, PaymentService paymentService)
    {
        this.dbContext = dbContext;
        this.inventoryService = inventoryService;
        this.discountService = discountService;
        this.paymentService = paymentService;
    }

    public async Task<ServiceResult> CheckoutAsync(POSDTO posdto, ClaimsGetter claims)
    {
        if (posdto == null || posdto.items == null || !posdto.items.Any())
            return ServiceResult.Fail("No items provided");

        if (posdto.amount_paid <= 0)
            return ServiceResult.Fail("Invalid payment amount");

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            int employeeId = Convert.ToInt32(claims.employeeId);
            int branchId = Convert.ToInt32(claims.branchId);
            string employeeDisplayId = claims.employeeDisplayId;
            string branchDisplayId = claims.branchDisplayId;
            string employeeName = claims.employeeName;

            var order = new SalesOrder
            {
                employee_id = employeeId,
                branch_id = branchId,
                sales_timestamp = DateTime.UtcNow
            };

            dbContext.SalesOrders.Add(order);
            await dbContext.SaveChangesAsync();

            var (total, orderDetails, error) =
                await inventoryService.ProcessItems(posdto.items, branchId, order.sales_order_id);

            if (error != null)
                return ServiceResult.Fail(error);

            total = await discountService.ApplyDiscount(posdto.discount_id, total);

            if (posdto.amount_paid < total)
                return ServiceResult.Fail("Amount paid is less than total.");

            order.total_amount = total;

            dbContext.SalesOrderDetails.AddRange(orderDetails);

            var payment = await paymentService.CreatePayment(order.sales_order_id, posdto, total);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var subtotal = orderDetails.Sum(x => x.unit_price * x.quantity);
            var vatableSales = Math.Round(total / 1.12m, 2);
            var vatAmount = Math.Round(total - vatableSales, 2);

            var receipt = new ReceiptDTO
            {
                receipt_number = order.sales_order_display_id,
                transaction_date = order.sales_timestamp,
                items = orderDetails.Select(od => new ReceiptItemDTO
                {
                    product_name = od.Products.product_name,
                    quantity = od.quantity,
                    unit_price = od.unit_price,
                    total_price = od.unit_price * od.quantity
                }).ToList(),

                subtotal = subtotal,
                discount_amount = subtotal - total,              
                total = total,
                payment_method = posdto.payment_method,
                amount_paid = payment.amount_paid,
                change = payment.change_amount,
                vatable_sales = vatableSales,
                vat_amount = vatAmount,
                branch = branchDisplayId,
                transaction_id = payment.payment_display_id,
                employee_id = employeeDisplayId,
                employee_name = employeeName
            };

            return ServiceResult.Ok(new
            {
                order.sales_order_display_id,
                payment.payment_display_id,
                receipt
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult.Fail(ex.Message);
        }
    }
}