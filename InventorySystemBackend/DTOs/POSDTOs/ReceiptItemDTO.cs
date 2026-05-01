namespace InventorySystemBackend.DTOs.POSDTOs
{
    public class ReceiptItemDTO
    {
        public string product_name { get; set; } = string.Empty;
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal total_price { get; set; }
    }
}
