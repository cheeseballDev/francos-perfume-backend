using System.Security.Claims;

namespace InventorySystemBackend.Services
{
    public class ClaimsGetter
    {
        private readonly ClaimsPrincipal user;

        public ClaimsGetter(ClaimsPrincipal user)
        {   
            this.user = user;
        }

        public string role =>
            user.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("Role claim missing");

        public string employeeId =>
            user.FindFirst("employee_id")?.Value
            ?? throw new UnauthorizedAccessException("Employee ID claim missing");

        public string employeeDisplayId =>
            user.FindFirst("employee_display_id")?.Value
            ?? throw new UnauthorizedAccessException("Employee display ID missing");

        public string branchId =>
            user.FindFirst("branch_id")?.Value
            ?? throw new UnauthorizedAccessException("Branch ID missing");

        public string branchDisplayId
        {
            get
            {
                if (role == "admin")
                    return "ADMIN";

                if(role == "owner")
                    return "OWNER";

                return user.FindFirst("branch_display_id")?.Value
                    ?? throw new UnauthorizedAccessException("Branch display ID missing");
            }
        }
    }
}