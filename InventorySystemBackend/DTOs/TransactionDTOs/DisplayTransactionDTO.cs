namespace InventorySystemBackend.DTOs.TransactionDTOs
{
    public class DisplayTransactionDTO
    {
        public string sales_order_id { get; set; }
        public List<TransactionItemDTO> product_list { get; set; }
        public string processed_by { get; set; }
        public decimal amount { get; set; }
        public DateTime transaction_date { get; set; }
    }
}
