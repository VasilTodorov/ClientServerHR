using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientServerHR.Models
{
    public class Employee
    {
        [BindNever]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please enter Employee first name")]
        [Display(Name = "First name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Employee last name")]
        [Display(Name = "Last name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Email address")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])",
            ErrorMessage = "The email address is not entered in a correct format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 1_000_000, ErrorMessage = "Salary must be between 0 and 1,000,000")]
        [Precision(18, 2)]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; } = default!;

    }
}
