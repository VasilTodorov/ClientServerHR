using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientServerHR.Models
{
    public class Employee
    {
        [BindNever]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary is required")]
        [Range(300, 1_000_000, ErrorMessage = "Salary must be between 300 and 1,000,000")]
        [Precision(18, 2)]
        public decimal Salary { get; set; }

        //[Required(ErrorMessage = "Department is required")]
        //[StringLength(50)]
        //public string Department { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = default!;
        public string ApplicationUserId { get; set; } = string.Empty;
        
        [BindNever]
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        public int CountryId { get; set; } // FK to Country
        public Country Country { get; set; } = null!;
        [MaxLength(256)]
        //public string EncryptedIban { get; set; } = string.Empty;

        //[NotMapped]
        public string? IBAN { get; set; } = string.Empty;

    }
}
