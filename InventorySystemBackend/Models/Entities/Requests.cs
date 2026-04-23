using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemBackend.Models.Entities
{
    [Table("request")]
    public class Requests
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int request_id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? request_display_id { get; set; }

        public int product_id { get; set; }
        public int employee_id { get; set; }

        public int branch_id { get; set; }
        public int delivered_to_branch_id { get; set; }

        public int request_qty { get; set; }
        public DateTime request_date_submitted { get; set; } = DateTime.UtcNow;

        public string? request_message { get; set; }
        public required string request_status { get; set; }

        public required string requested_from { get; set; }  
        public required string delivered_to { get; set; }
        public required string delivery_type { get; set; }

        [ForeignKey("product_id")]
        public virtual Products Product { get; set; }
        [ForeignKey("employee_id")]
        public virtual EmployeeProfiles Employee { get; set; } 

        [ForeignKey("branch_id")]
        public virtual Branches FromBranch { get; set; }

        [ForeignKey("delivered_to_branch_id")]
        public virtual Branches ToBranch { get; set; }
    }
}