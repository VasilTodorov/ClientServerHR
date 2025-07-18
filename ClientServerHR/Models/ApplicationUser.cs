using Microsoft.AspNetCore.Identity;

namespace ClientServerHR.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}
