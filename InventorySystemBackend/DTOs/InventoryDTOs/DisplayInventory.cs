namespace InventorySystemBackend.DTOs.InventoryDTOs
{
    public class DisplayInventory
    {
        public int product_id { get; set; }
        public string? product_display_id { get; set; }
        public string? product_name { get; set; }
        public string? product_type { get; set; }
        public string? product_note { get; set; }
        public string? product_gender { get; set; }
        public string? product_barcode { get; set; }
        public string? product_status { get; set; }
        public decimal product_price { get; set; }
        public string? product_image_url { get; set; }
        public DateTime product_date_created { get; set; }
        public string? branch_display_id { get; set; }
        public string? branch_name { get; set; }
        //public string? branch_display_id { get; set; }
        public int product_qty { get; set; }
    }
}
