namespace InventorySystemBackend.DTOs.ArchiveDisplayDTOs
{
    public class ArchivedAccountDisplayDTO
    {
        public string account_archive_display_id { get; set; }
        public string employee_display_id { get; set; }
        public int branch_id { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string archived_by{ get; set; }
        public DateTime date_archived { get; set; }
    }
}
