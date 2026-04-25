using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemBackend.DTOs
{
    public class AuditLogDisplayDTO
    {
        public string? log_display_id { get; set; }

        public string? employee_display_id { get; set; }
        public string? branch_display_id { get; set; }

        public string? log_action { get; set; }
        public string? log_module { get; set; }

        public DateTime log_timestamp { get; set; }
    }
}
