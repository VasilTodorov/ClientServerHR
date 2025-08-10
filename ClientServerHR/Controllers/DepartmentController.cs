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

        [Authorize(Roles = "manager,admin")]
        public IActionResult PieChart()
        {
            var list = _departmentRepository.AllDepartments.Select(d=> new DepartmentPieViewModel
            {
                Name = d.Name,
                Count = d.Employees.Count,
            }).OrderBy(p=>p.Count).ToList();

            int sum = list.Sum(p=>p.Count);
            foreach(DepartmentPieViewModel departmentPie in list)
            {
                departmentPie.Percent = (departmentPie.Count/ Convert.ToDouble(sum)) * 100;
            }
            //var list = _departmentRepository.AllDepartments.ToList();
            return View(list);
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
