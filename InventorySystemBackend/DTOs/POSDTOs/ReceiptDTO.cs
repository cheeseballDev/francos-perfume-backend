namespace InventorySystemBackend.DTOs.POSDTOs
{
    public class ReceiptDTO
    {
        public string receipt_number { get; set; } = string.Empty;
        public DateTime transaction_date { get; set; }

        public string payment_method { get; set; } = string.Empty;

        public List<ReceiptItemDTO> items { get; set; } = new();

        public decimal subtotal { get; set; }
        public decimal discount_amount { get; set; }
        public decimal vatable_sales { get; set; }
        public decimal vat_amount { get; set; }
        public decimal total { get; set; }

        public decimal amount_paid { get; set; }
        public decimal change { get; set; }
        public string branch { get; set; } = string.Empty;
        public string transaction_id {  get; set; } = string.Empty;
        public string employee_id { get; set; } = string.Empty;
        public string employee_name { get; set; } = string.Empty;
    }
}
