using System.IO.Pipelines;

namespace ClientServerHR.Repositories
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; } = default!;
    }
}
