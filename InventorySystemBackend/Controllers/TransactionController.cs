using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.TransactionDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

public class TransactionController : ControllerBase
    {
    private readonly DatabaseContext dbContext;

    public TransactionController(DatabaseContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet("displayTransactions")]
    public async Task<IActionResult> DisplayTransactions(int page = 1, int pageSize = 20)
    {
        var claims = new ClaimsGetter(User);
        var userRole = claims.role;
        var userBranchId = int.Parse(claims.branchId);
        var query = dbContext.SalesOrders.AsQueryable();

        if (userRole != "ADMIN" && userRole != "OWNER")
        {
            query = query.Where(so => so.branch_id == userBranchId);
        }

        var transactionList = await query
            .OrderByDescending(so => so.sales_timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(so => new
            {
                so.sales_order_display_id,
                so.sales_timestamp,
                employee = so.Employee.employee_full_name,
                amount = so.Payments.Select(p => p.amount_paid).FirstOrDefault(),
                items = so.Items.Select(i => new
                {
                    name = i.Products.product_name,
                    qty = i.quantity
                }).ToList()
            })
            .ToListAsync();

        var transactionRow = transactionList.Select(x => new DisplayTransactionDTO
        {
            sales_order_id = x.sales_order_display_id,
            processed_by = x.employee,
            transaction_date = x.sales_timestamp,
            amount = x.amount,
            product_list = x.items.Select(i => new TransactionItemDTO
            {
                product_name = i.name,
                qty = i.qty
            }).ToList()
        }).ToList();
        return Ok(transactionRow);
    }
}

