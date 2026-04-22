using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.ArchiveDisplayDTOs;
using InventorySystemBackend.Services;
using InventorySystemBackend.Services.ArchivingServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> DisplayArchivedAccounts()
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchId = int.Parse(claims.branchId);

            var query = dbContext.ArchivedAccounts.AsQueryable();

            if (role != "ADMIN")
            {
                query = query.Where(i => i.branch_id == branchId);
            }

            var employeeArchiveList = await query.ToListAsync();
            var displayList = new List<ArchivedAccountDisplayDTO>();
            foreach (var archivedEmployee in employeeArchiveList)
            {
                displayList.Add(new ArchivedAccountDisplayDTO
                {
                    account_archive_display_id = archivedEmployee.account_archive_display_id,
                    employee_display_id = archivedEmployee.employee_display_id,
                    branch_id = archivedEmployee.branch_id,
                    email = archivedEmployee.email,
                    role = archivedEmployee.employee_role,
                    archived_by = archivedEmployee.archived_by,
                    date_archived = archivedEmployee.date_archived
                });
            }

            return Ok(displayList);
        }

        [HttpGet("displayArchivedProducts")]
        public async Task<IActionResult> DisplayArchivedProducts()
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchId = int.Parse(claims.branchId);

            var productArchiveList = await dbContext.ArchivedProducts.ToListAsync();

            var displayList = new List<ArchivedProductDisplayDTO>();
            foreach (var archivedProduct in productArchiveList)
            {
                displayList.Add(new ArchivedProductDisplayDTO
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
                });
            }

            return Ok(displayList);
        }
    }
}
