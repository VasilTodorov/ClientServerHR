using ClientServerHR.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string? Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary is required")]
        [Range(300, 1_000_000)]
        public decimal? Salary { get; set; }
        [Required(ErrorMessage = "Department is required")]
        [StringLength(50)]
        public string? DepartmentName { get; set; }

        [BindNever]
        [ValidateNever]
        public List<Department> Departments { get; set; } = default!;

        
        [ValidateNever]
        public int? ViewDepartmentId { get; set; }
        [Required(ErrorMessage = "Country is required")]
        public int CountryId { get; set; }

        public IEnumerable<SelectListItem> CountryOptions { get; set; } = new List<SelectListItem>();
    }

}
