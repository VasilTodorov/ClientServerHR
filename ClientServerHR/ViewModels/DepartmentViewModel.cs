using ClientServerHR.Models;
using System.IO.Pipelines;

namespace ClientServerHR.ViewModels
{
    public class DepartmentViewModel
    {
        public IEnumerable<Employee> Employees { get; }
        public string? CurrentDepartment { get; }

        public DepartmentViewModel(IEnumerable<Employee> employees, string? currentDepartment)
        {
            Employees = employees;
            CurrentDepartment = currentDepartment;
        }
    }
}
