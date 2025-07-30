namespace ClientServerHR.Models
{
    public interface IDepartmentRepository
    {
        IEnumerable<Department> AllDepartments { get; }
        Department? GetDepartmentById(int departmentId);
        Department? GetDepartmentByName(string departmentName);

        public void AddDepartment(Department department);

        public void Update(Department department);

        public void Delete(int id);
    }
}
