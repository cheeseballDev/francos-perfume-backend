namespace InventorySystemBackend.DTOs.RequestDTOs
{
    public class CreateRequestDTO
    {
        public int product_id { get; set; }
        public int request_qty { get; set; }

        public string? request_message { get; set; }
        public int delivered_to_branch_id { get; set; }
        public string? delivery_type { get; set; }
    }
}
