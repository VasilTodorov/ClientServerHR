using ClientServerHR.Models;
using ClientServerHR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientServerHR.Controllers
{
    //[Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public IActionResult List()
        {
            return View(_employeeRepository.AllEmployees);
        }
        public IActionResult Department(string? department)
        {
            //string department = "Junior";
            if (string.IsNullOrEmpty(department))
            {
                return NotFound("Department is required.");
            }
            DepartmentViewModel myDepartment = new DepartmentViewModel(_employeeRepository.GetAllEmployeesByDepartment(department), department);
            return View(myDepartment);
        }
        public IActionResult Detail(int id)
        {
            var employee = _employeeRepository.GetEmployeeById(id);
            if (employee == null)
                return NotFound();
            return View(employee);
        }
        [HttpGet]
        public IActionResult HireEmployee()
        {
            return View();
        }
        [HttpPost]
        public IActionResult HireEmployee(Employee emp)
        {           
            if (ModelState.IsValid)
            {                
                _employeeRepository.AddEmployee(emp);
                return RedirectToAction("HireComplete");
            }
            return View(emp);
        }

        public IActionResult HireComplete()
        {
            ViewBag.HireCompleteMessage = "Employee is hired";
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
