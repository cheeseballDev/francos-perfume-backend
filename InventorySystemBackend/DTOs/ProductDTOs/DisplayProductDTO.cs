namespace InventorySystemBackend.DTOs.ProductDTOs
{
    public class DisplayProductDTO
    {
        public string? product_display_id { get; set; }
        public string? product_name { get; set; }
        public string? product_type { get; set; }

        public string? product_note { get; set; }
        public string? product_gender { get; set; }

        public DateTime product_date_created { get; set; } = DateTime.UtcNow;
        public string? product_barcode { get; set; }
        public string? product_description { get; set; }
        public decimal product_price { get; set; } = 0.0m;
        public string? product_image_url { get; set; }

    }
}
