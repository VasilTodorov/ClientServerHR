namespace ClientServerHR.ViewModels
{
    public class DepartmentWithRoleViewModel
    {
        public string Title = string.Empty;
        public int? DepartmentId;
        public List<EmployeeWithRoleViewModel> Employees { get; set; } = new(); 

    }
}
