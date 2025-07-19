using System.IO.Pipelines;

namespace ClientServerHR.Models
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> AllEmployees { get; }
        IEnumerable<Employee> GetAllEmployeesByDepartment(string department);
        Employee? GetEmployeeById(int employeeId);

        public void AddEmployee(Employee employee);

        public void Update(Employee employee);

        public void Delete(int id);
        
    }
}
