using ClientServerHR.Models;

namespace ClientServerHR.ViewModels
{
    public class DepartmentWithRoleViewModel
    {
        public string DepartmentName = string.Empty;
        public int? DepartmentId;
        public List<EmployeeWithRoleViewModel> Employees { get; set; } = new(); 

    }
}
