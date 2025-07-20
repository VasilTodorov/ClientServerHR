using System.ComponentModel.DataAnnotations;

namespace ClientServerHR.ViewModels
{
    // ViewModels/UserProfileEditViewModel.cs
    public class UserProfileEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Range(0, 1_000_000)]
        public decimal? Salary { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }
    }

}
