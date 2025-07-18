using System.IO.Pipelines;

namespace ClientServerHR.Models
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> AllEmployees { get; }
        IEnumerable<Employee> GetAllEmployeesByDepartment(string department);
        Employee? GetEmployeeById(int employeeId);

        public void AddEmployee(Employee employee);

        public void UpdateAsync(Employee employee);

        public void DeleteAsync(int id);
        
    }
}
