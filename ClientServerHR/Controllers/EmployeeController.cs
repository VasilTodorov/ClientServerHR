using ClientServerHR.Models;
using ClientServerHR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientServerHR.Controllers
{
    //[Authorize(Roles = "employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(IEmployeeRepository employeeRepository, UserManager<ApplicationUser> userManager)
        {
            _employeeRepository = employeeRepository;
            _userManager = userManager;
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
        [Authorize]
        public IActionResult Profile(string? userId)
        {
            if (userId == null)
                userId = _userManager.GetUserId(User);

            ApplicationUser? user = _userManager.Users.Include(u => u.Employee).FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
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
