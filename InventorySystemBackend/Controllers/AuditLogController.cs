using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs;
using InventorySystemBackend.DTOs.ArchiveDisplayDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly DatabaseContext dbContext;
        public AuditLogController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("displayAll")]
        public async Task<IActionResult> DisplayAuditLogs(int page = 1, int pageSize = 20)
        {
            var claims = new ClaimsGetter(User);
            var role = claims.role;
            var branchDisplayId = claims.branchDisplayId;

            var query = dbContext.AuditLogs.AsQueryable();

            if (role != "ADMIN" && role != "OWNER")
            {
                query = query.Where(i => i.branch_display_id == branchDisplayId);
            }

            var totalAuditLogs = await query.CountAsync();
            var totalAuditLogsPages = (int)Math.Ceiling(totalAuditLogs / (double)pageSize);
            var allAuditLogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var displayList = allAuditLogs.Select(allAuditLogs => new AuditLogDisplayDTO
            {
                log_display_id = allAuditLogs.log_display_id,
                employee_display_id = allAuditLogs.employee_display_id,
                branch_display_id = allAuditLogs.branch_display_id,
                log_action = allAuditLogs.log_action,
                log_module = allAuditLogs.log_module,
                log_timestamp = allAuditLogs.log_timestamp
            }).ToList();

            return Ok(new
            {
                totalAuditLogs,
                totalAuditLogsPages,
                page,
                pageSize,
                data = displayList
            });
        }

        [HttpGet("displayOneAuditLog")]
        public async Task<IActionResult> AuditLogDetails(int id)
        {
            var displayAuditLog = await dbContext.AuditLogs
                .Where(i => i.log_id == id)
                .Select(i => new AuditLogDisplayDTO
                {
                    log_display_id = i.log_display_id,
                    employee_display_id = i.employee_display_id,
                    branch_display_id = i.branch_display_id,
                    log_action = i.log_action,
                    log_module = i.log_module,
                    log_timestamp = i.log_timestamp
                })
                .FirstOrDefaultAsync();
            if (displayAuditLog == null)
            {
                return NotFound(new { message = "Audit Log not found" });
            }
            return Ok(displayAuditLog);
        }
    }
}