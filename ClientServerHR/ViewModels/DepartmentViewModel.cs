using ClientServerHR.Models;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;

namespace ClientServerHR.ViewModels
{
    public class DepartmentViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        [Display(Name = "Department Name")]
        public string? DepartmentName { set; get; }       
    }
}
