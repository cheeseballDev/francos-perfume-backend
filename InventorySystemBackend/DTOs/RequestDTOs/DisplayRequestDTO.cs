namespace InventorySystemBackend.DTOs.RequestDTOs
{
    public class DisplayRequestDTO
    {
        public string? request_display_id { get; set; }

        public string? product_name { get; set; }
        public string? employee_display_id { get; set; }
        public int request_qty { get; set; }
        public DateTime request_date_submitted { get; set; }

        public string? request_message { get; set; }
        public string? request_status { get; set; }
        public string? requested_from { get; set; }
        public string? delivered_to { get; set; }
        public string? delivery_type { get; set; }
    }
}
