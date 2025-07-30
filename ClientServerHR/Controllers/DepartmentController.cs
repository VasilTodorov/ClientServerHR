using ClientServerHR.Models;
using ClientServerHR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClientServerHR.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentController(IDepartmentRepository departmentRepository, UserManager<ApplicationUser> userManager)
        {            
            _departmentRepository = departmentRepository;
            _userManager = userManager;           
        }

        [Authorize(Roles = "manager,admin")]
        public IActionResult List()
        {
            var list = _departmentRepository.AllDepartments.ToList() ;
            return View(list);
        }
        public IActionResult Display(int departmentId)
        {
            var department = _departmentRepository.GetDepartmentById(departmentId);
            if(department == null)
            {
                return NotFound();
            }
            var employees = department.Employees;
            var model = new DepartmentWithRoleViewModel();
            
            foreach (var emp in employees!)
            {
                var roles = _userManager.GetRolesAsync(emp.ApplicationUser).Result;
                model.Employees.Add(new EmployeeWithRoleViewModel
                {
                    Employee = emp,
                    Roles = roles.ToList()
                });
            }
            model.DepartmentName = department.Name;
            model.DepartmentId = departmentId;
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {            

            var departmentViewModel = new DepartmentViewModel();
            return View(departmentViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(DepartmentViewModel model)
        {
            if (model.DepartmentName == null) ModelState.AddModelError("DepartmentName", "Department cant be null");
            else if (_departmentRepository.GetDepartmentByName(model.DepartmentName) != null)
            {
                ModelState.AddModelError("DepartmentName", "department must have a unique name.");
            }
            if (ModelState.IsValid)
            {
                
                var department = new Department()
                {
                    Name = model.DepartmentName!
                };
                _departmentRepository.AddDepartment(department);
                return RedirectToAction("List");
            }
            else
            {
                return View();
            }
                
        }
    }
}
