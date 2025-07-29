
using Microsoft.EntityFrameworkCore;

namespace ClientServerHR.Models
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ClientServerHRDbContext _clientServerHRDbContext;

        public EmployeeRepository(ClientServerHRDbContext clientServerHRDbContext)
        {
            _clientServerHRDbContext = clientServerHRDbContext;
        }

        public IEnumerable<Employee> AllEmployees
        {
            get
            {
                return _clientServerHRDbContext.Employees.Include(e=>e.ApplicationUser).Include(e=>e.Department);
            }
        }
        //public IEnumerable<Employee> GetAllEmployeesByDepartment(string department)
        //{
        //    return _clientServerHRDbContext.Employees.Where(p => p.Department == department).Include(e => e.ApplicationUser);
        //}
        public Employee? GetEmployeeById(int employeeId)
        {
            return _clientServerHRDbContext.Employees.Include(e=>e.ApplicationUser).FirstOrDefault(p => p.EmployeeId == employeeId);
        }

        public void AddEmployee(Employee employee)
        {
            _clientServerHRDbContext.Employees.Add(employee);
            _clientServerHRDbContext.SaveChanges();
        }

        public void Update(Employee employee)
        {
            _clientServerHRDbContext.Employees.Update(employee);
            _clientServerHRDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var employee = _clientServerHRDbContext.Employees.Find(id);
            if (employee != null)
            {
                _clientServerHRDbContext.Employees.Remove(employee);
                _clientServerHRDbContext.SaveChanges();
            }
        }

    }
}
