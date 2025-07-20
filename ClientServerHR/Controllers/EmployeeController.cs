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
        public IActionResult Applicants()
        {
            var applicants = _userManager.Users.Where(u => u.Employee == null).ToList();
            return View(applicants);
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
        public IActionResult NotRegistered()
        {
            return View();
        }

        public IActionResult MyProfile()
        {
            if (User.Identity?.IsAuthenticated!=true)
            {
                return RedirectToAction("NotRegistered", "Employee");
            }
            var  userId = _userManager.GetUserId(User);

            ApplicationUser? user = _userManager.Users.Include(u => u.Employee).FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }
        //[Authorize]
        //public IActionResult Profile(string? userId)
        //{       

        //    ApplicationUser? user = _userManager.Users.Include(u => u.Employee).FirstOrDefault(u => u.Id == userId);

        //    if (user == null)
        //        return NotFound();

        //    return View(user);
        //}
        [Authorize(Roles = "manager,admin")]
        public IActionResult Profile(string? userId)
        {

            //if (userId == null)
            //    userId = _userManager.GetUserId(User);

            var user = _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // Check if current user can edit
            bool isEditable = false;
            if(User.IsInRole("manager"))
            {
                var currentUserId = _userManager.GetUserId(User);
                var currentUser = _userManager.Users
                    .Include(u => u.Employee)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if(currentUser?.Employee?.Department != null && user.Employee?.Department!=null)
                    isEditable = currentUser.Employee.Department == user.Employee.Department;
            }
            else if(User.IsInRole("admin"))
            {
                isEditable = true;
            }
            if (!isEditable)
                return Forbid();

            ViewData["IsEditable"] = isEditable;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public IActionResult Edit(ApplicationUser model)
        {
            var user = _userManager.Users.Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == model.Id);

            if (user == null) return NotFound();

            // Only allow current user or admin to update
            var currentUserId = _userManager.GetUserId(User);

            if (!User.IsInRole("manager") && !User.IsInRole("admin"))
            {
                return Forbid(); // 403 Forbidden
            }

            if (User.IsInRole("manager"))
            {
                //var currentUserId = _userManager.GetUserId(User);
                var currentUser = _userManager.Users
                    .Include(u => u.Employee)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser?.Employee?.Department != null && user.Employee?.Department != null)
                {
                    if (currentUser.Employee.Department != user.Employee.Department)
                    {
                        return Forbid();
                    }
                    else if(currentUser.Employee.Department == user.Employee.Department &&
                            user.Employee.Department != model.Employee?.Department )
                    {
                        return Forbid();
                    }
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            if (user.Employee != null && model.Employee != null)
            {
                user.Employee.Salary = model.Employee.Salary;
                user.Employee.Department = model.Employee.Department;
            }

            _userManager.UpdateAsync(user).Wait();
            return RedirectToAction("Profile", new { userId = user.Id });
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
            return RedirectToAction("MyProfile");
        }
    }
}
