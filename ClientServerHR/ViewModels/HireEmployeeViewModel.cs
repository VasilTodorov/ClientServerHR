using ClientServerHR.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace ClientServerHR.ViewModels
{
    public class HireEmployeeViewModel
    {        

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string? Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary is required")]
        [Range(300, 1_000_000, ErrorMessage = "Salary must be between 300 and 1,000,000")]
        [Precision(18, 2)]
        public decimal? Salary { get; set; }
        
        [Required(ErrorMessage = "Department is required")]
        [StringLength(50)]
        public string? DepartmentName { get; set; } = string.Empty;        
        public string ApplicationUserId { get; set; } = string.Empty;       

        [BindNever]
        [ValidateNever]
        public List<Department> Departments { get; set; } = default!;

        [Required(ErrorMessage = "Country is required")]
        public int? CountryId { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$",
        ErrorMessage = "IBAN format is invalid.")]
        public string IBAN { get; set; } = string.Empty;
        [BindNever]
        [ValidateNever]
        public List<SelectListItem> CountryOptions { get; set; } = new List<SelectListItem>();
    }
}
