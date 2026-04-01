using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemBackend.Models.Entities
{
    [Table("auditlogtable")]
    public class AuditLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int log_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public required string log_display_id { get; set; }

        public int employee_id { get; set; }
        public int branch_id { get; set; }

        public required string location { get; set; }
        public required string log_details { get; set; }
        public required string log_module { get; set; }

        public DateTime log_timestamp { get; set; } = DateTime.UtcNow;
    }
}