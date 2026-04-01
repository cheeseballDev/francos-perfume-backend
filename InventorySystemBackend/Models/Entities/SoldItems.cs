using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemBackend.Models.Entities
{
    [Table("solditemstable")]
    public class SoldItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int sold_item_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public required string sold_item_display_id { get; set; }

        public int sales_id { get; set; }
        public int product_id { get; set; }

        public int sold_qty { get; set; }
        public decimal sales_price { get; set; }
    }
}