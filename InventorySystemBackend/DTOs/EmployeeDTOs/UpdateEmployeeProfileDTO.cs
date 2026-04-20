namespace InventorySystemBackend.DTOs.EmployeeDTOs
{
    public class UpdateEmployeeProfileDTO
    {
        public int branch_id { get; set; }
        public required string contact_number { get; set; }
        public required string address { get; set; }
        public required string first_name { get; set; }
        public string? middle_name { get; set; }
        public required string last_name { get; set; }
        public required string employee_shift { get; set; }
        public required string employee_profile_picture { get; set; }

    }
}
