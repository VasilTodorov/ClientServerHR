using ClientServerHR.Models;

namespace ClientServerHR.ViewModels
{
    public class EmployeeWithRoleViewModel
    {
        public Employee Employee { get; set; }
        public List<string> Roles { get; set; } = new(); // For multiple roles, or string for one role

    }
}
