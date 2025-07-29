
using Microsoft.EntityFrameworkCore;

namespace ClientServerHR.Models
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ClientServerHRDbContext _clientServerHRDbContext;

        public DepartmentRepository(ClientServerHRDbContext clientServerHRDbContext)
        {
            _clientServerHRDbContext = clientServerHRDbContext;
        }
        public IEnumerable<Department> AllDepartments
        {
            get
            {
                return _clientServerHRDbContext.Departments;
            }
        }

        public void AddDepartment(Department department)
        {
            _clientServerHRDbContext.Departments.Add(department);
            _clientServerHRDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var result = _clientServerHRDbContext.Departments.Find(id);
            if (result != null)
            {
                _clientServerHRDbContext.Departments.Remove(result);
                _clientServerHRDbContext.SaveChanges();
            }
        }

        public Department? GetDepartmentById(int departmentId)
        {
            var result = _clientServerHRDbContext.Departments.FirstOrDefault(d=> d.DepartmentId == departmentId);            
            return result;
        }

        public void Update(Department department)
        {
            throw new NotImplementedException();
        }
    }
}
