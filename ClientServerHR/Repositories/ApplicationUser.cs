using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClientServerHR.Repositories
{
    public class ApplicationUser : IdentityUser
    {        
        [PersonalData]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        public Employee? Employee { get; set; } = default!;
    }
}
