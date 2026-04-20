namespace InventorySystemBackend.DTOs.EmployeeDTOs
{
    public class DisplayEmployeeProfileDTO
    {
        public int employee_id { get; set; }
        public string employee_display_id { get; set; }
        public int branch_id { get; set; }
        public string branch_display_id { get; set; }
        public string employee_full_name { get; set; }
        public string contact_number { get; set; }
        public string address { get; set; }
        public string employee_shift { get; set; }
        public DateTime account_created { get; set; }
        public string employee_profile_picture { get; set; }
    }
}
