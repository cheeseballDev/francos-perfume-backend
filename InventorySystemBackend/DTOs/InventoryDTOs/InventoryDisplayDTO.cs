namespace InventorySystemBackend.DTOs.InventoryDTOs
{
    public class InventoryDisplayDTO
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

<<<<<<< HEAD
        public string? branch_name { get; set; }
        //public string? branch_display_id { get; set; }
=======
        //public string? branch_display_id { get; set; }
        public string? branch_name { get; set; }
>>>>>>> 7df4d5774448c9cce27e8bb8ddea57448a0c9c3b
        public int product_qty { get; set; }
    }
}
