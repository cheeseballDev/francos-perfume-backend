namespace InventorySystemBackend.DTOs.ArchiveDisplayDTOs
{
    public class ArchivedProductDisplayDTO
    {
        public string product_archive_display_id { get; set; }

        public string product_display_id { get; set; }

        public string product_name { get; set; }

        public string product_type { get; set; }

        public string product_note { get; set; }

        public string product_gender { get; set; }

        public string product_barcode { get; set; }

        public string archived_by { get; set; }

        public DateTime date_archived { get; set; }
    }
}
