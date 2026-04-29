using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.ArchiveDisplayDTOs;
using InventorySystemBackend.DTOs.EmployeeDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.ArchivingServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivingController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        private readonly AccountArchivingService accountArchivingService;
        private readonly ProductArchivingService productArchivingService;
        public ArchivingController(DatabaseContext dbContext, AccountArchivingService accountArchivingService, ProductArchivingService productArchivingService)
        {
            this.dbContext = dbContext;
            this.accountArchivingService = accountArchivingService;
            this.productArchivingService = productArchivingService;
        }

        [HttpPut("archiveAccount/{id:int}")]
        public async Task<IActionResult> ArchiveAccount(int id)
        {
            var claims = new ClaimsGetter(User);

            var result = await accountArchivingService.ArchiveAccount(id, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("archiveProduct/{id:int}")]
        public async Task<IActionResult> ArchiveProduct(int id)
        {
            var claims = new ClaimsGetter(User);

            var result = await productArchivingService.ArchiveProduct(id, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("archivedAccountDetails/{id:int}")]
        public async Task<IActionResult> ArchiveAccountDetails(int id)
        {
            var archivedAccount = dbContext.ArchivedAccounts
                .Where(i => i.account_archive_id == id)
                .Select(i => new ArchivedAccountDisplayDTO
                {
                    account_archive_display_id = i.account_archive_display_id,
                    employee_display_id = i.employee_display_id,
                    branch_id = i.branch_id,
                    email = i.email,
                    role = i.employee_role,
                    archived_by = i.archived_by,
                    date_archived = i.date_archived
                })
                .FirstOrDefault();

            if (archivedAccount == null)
            {
                return NotFound(new { message = "Account not found" });
            }

            return Ok(archivedAccount);
        }

        [HttpPut("archivedProductDetails/{id:int}")]
        public async Task<IActionResult> ArchiveProductDetails(int id)
        {
            var archivedProduct = dbContext.ArchivedProducts
                .Where(i => i.product_archive_id == id)
                .Select(i => new ArchivedProductDisplayDTO
                {
                    product_archive_display_id = i.product_archive_display_id,
                    product_display_id = i.product_display_id,
                    product_name = i.product_name,
                    product_type = i.product_type,
                    product_note = i.product_note,
                    product_gender = i.product_gender,
                    product_barcode = i.product_barcode,
                    archived_by = i.archived_by,
                    date_archived = i.date_archived
                })
                .FirstOrDefault();

            if (archivedProduct == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(archivedProduct);
        }

        [HttpGet("displayArchivedAccounts")]
        public async Task<IActionResult> DisplayArchivedAccounts(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchId = int.Parse(claims.branchId);

            var query = dbContext.ArchivedAccounts.AsQueryable();

            if (role != "ADMIN" && role != "OWNER")
            {
                query = query.Where(i => i.branch_id == branchId);
            }

            var totalAccounts = await query.CountAsync();
            var totalAccountsPages = (int)Math.Ceiling(totalAccounts / (double)pageSize);

            var accounts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var displayList = accounts.Select(archivedEmployee => new ArchivedAccountDisplayDTO
            {
                account_archive_display_id = archivedEmployee.account_archive_display_id,
                employee_display_id = archivedEmployee.employee_display_id,
                branch_id = archivedEmployee.branch_id,
                email = archivedEmployee.email,
                role = archivedEmployee.employee_role,
                archived_by = archivedEmployee.archived_by,
                date_archived = archivedEmployee.date_archived
            }).ToList();

            return Ok(new
            {
                totalAccounts,
                totalAccountsPages,
                page,
                pageSize,
                data = displayList
            });
        }

        [HttpGet("displayArchivedProducts")]
        public async Task<IActionResult> DisplayArchivedProducts(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchId = int.Parse(claims.branchId);

            var totalProducts = await dbContext.ArchivedProducts.CountAsync();
            var totalProductsPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await dbContext.ArchivedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var displayList = products.Select(archivedProduct => new ArchivedProductDisplayDTO
            {
                product_archive_display_id = archivedProduct.product_archive_display_id,
                product_display_id = archivedProduct.product_display_id,
                product_name = archivedProduct.product_name,
                product_type = archivedProduct.product_type,
                product_note = archivedProduct.product_note,
                product_gender = archivedProduct.product_gender,
                product_barcode = archivedProduct.product_barcode,
                archived_by = archivedProduct.archived_by,
                date_archived = archivedProduct.date_archived
            }).ToList();

            return Ok(new
            {
                totalProducts,
                totalProductsPages,
                page,
                pageSize,
                data = displayList
            });
        }
    }
}
