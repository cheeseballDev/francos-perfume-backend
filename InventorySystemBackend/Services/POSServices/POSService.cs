using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.POSDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;

public class POSService
{
    private readonly DatabaseContext dbContext;
    private readonly InventoryService inventoryService;
    private readonly DiscountService discountService;
    private readonly PaymentService paymentService;

    public POSService(DatabaseContext dbContext, InventoryService inventoryService, DiscountService discountService, PaymentService paymentService)
    {
        this.dbContext = dbContext;
        this.inventoryService = inventoryService;
        this.discountService = discountService;
        this.paymentService = paymentService;
    }

    public async Task<ServiceResult> CheckoutAsync(POSDTO dto, ClaimsGetter claims)
    {
        if (dto == null || dto.items == null || !dto.items.Any())
            return ServiceResult.Fail("No items provided");

        if (dto.amount_paid <= 0)
            return ServiceResult.Fail("Invalid payment amount");

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            int employeeId = Convert.ToInt32(claims.employeeId);
            int branchId = Convert.ToInt32(claims.branchId);

            var order = new SalesOrder
            {
                employee_id = employeeId,
                branch_id = branchId,
                sales_timestamp = DateTime.UtcNow
            };

            dbContext.SalesOrders.Add(order);
            await dbContext.SaveChangesAsync();

            var (total, orderDetails, error) =
                await inventoryService.ProcessItems(dto.items, branchId, order.sales_order_id);

            if (error != null)
                return ServiceResult.Fail(error);

            total = await discountService.ApplyDiscount(dto.discount_id, total);

            if (dto.amount_paid < total)
                return ServiceResult.Fail("Amount paid is less than total.");

            order.total_amount = total;

            dbContext.SalesOrderDetails.AddRange(orderDetails);

            var payment = await paymentService.CreatePayment(order.sales_order_id, dto, total);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Ok(new
            {
                order.sales_order_display_id,
                payment.payment_display_id,
                total,
                payment.amount_paid,
                payment.change_amount
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult.Fail(ex.Message);
        }
    }
}