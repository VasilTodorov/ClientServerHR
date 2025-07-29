using ClientServerHR.Models;
using ClientServerHR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ClientServerHR.Controllers
{
    //[Authorize(Roles = "employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;

        private static readonly List<string> AllowedDepartments = new()
        {
            "HR", "IT", "Finance", "Sales", "Marketing"
        };
        private static readonly List<string> AllowedPositions = new()
        {
            "Junior Dev", "Mid Dev", "Senior dev", "Devops" 
        };

        public EmployeeController(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, UserManager<ApplicationUser> userManager, ILogger<EmployeeController> logger)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _userManager = userManager;
            _logger = logger;
        }
        [Authorize(Roles = "manager,admin")]
        public IActionResult List()
        {
            List<Employee> employees;
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var user = _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == userId);

                if (user == null)
                    return NotFound();

                var roles = _userManager.GetRolesAsync(user!).Result;
                if(roles.Any(r=>r== "manager"))
                {
                    employees = _employeeRepository.AllEmployees.Where(e=>e.Department==user.Employee?.Department).ToList();
                }
                else
                {
                    employees = _employeeRepository.AllEmployees.ToList();
                }
            }
            else
            {
                _logger.LogInformation("EmployeeController.List called with invalid user id: {UserId}", userId);
                return NotFound();
            }

            var model = new List<EmployeeWithRoleViewModel>();

            foreach (var emp in employees)
            {
                var roles = _userManager.GetRolesAsync(emp.ApplicationUser).Result;
                model.Add(new EmployeeWithRoleViewModel
                {
                    Employee = emp,
                    Roles = roles.ToList()
                });
            }

            return View(model);
            
        }
        [Authorize(Roles = "manager,admin")]
        public IActionResult Applicants()
        {
            var applicants = _userManager.Users.Where(u => u.Employee == null).ToList();
            return View(applicants);
        }

        public IActionResult NotRegistered()
        {
            return View();
        }

        [Authorize(Roles = "manager,admin")]
        public IActionResult Delete(string userId)
        {
            var user = _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            if(user.Employee != null)            
            {
                _employeeRepository.Delete(user.Employee.EmployeeId);
                TempData["Message"] = "Employee deleted successfully.";

                var result = _userManager.DeleteAsync(user).Result;

                if (result.Succeeded)
                {
                    this._logger.LogInformation("EmployeeController.Delete {UserId} was deleted by {DeleterUserId}", userId, user.Id);
                    TempData["Message"] = "Employee deleted successfully.";
                    return RedirectToAction("List"); // Or wherever you list users
                }

                return RedirectToAction("List");
            }
            
            var resultApplicant = _userManager.DeleteAsync(user).Result;
            if (resultApplicant.Succeeded)
            {
                TempData["Message"] = "Applicant deleted successfully.";
                return RedirectToAction("Applicants"); // Or wherever you list users
            }

            return RedirectToAction("Applicants");
            
        }
        public IActionResult MyProfile()
        {
            if (User.Identity?.IsAuthenticated!=true)
            {
                return RedirectToAction("NotRegistered", "Employee");
            }
            var  userId = _userManager.GetUserId(User);

            ApplicationUser? user = _userManager.Users.Include(u => u.Employee).ThenInclude(e=>e.Department).FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogInformation("EmployeeController.MyProfile called with invalid user id: {UserId}", userId);
                return NotFound();
            }

            return View(user);
        }
        
        [Authorize(Roles = "manager,admin")]
        public IActionResult Profile(string? userId)
        {
            var user = _userManager.Users
                .Include(u => u.Employee)
                .ThenInclude(e => e.Department)
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
            {
                _logger.LogInformation("EmployeeController.Profile called with invalid with no permissions user id: {UserId}", userId);
                return Forbid();
            }
                

            var viewModel = new UserProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Position = user.Employee?.Position,
                Salary = user.Employee?.Salary,
                DepartmentName = user.Employee?.Department.Name
            };

            ViewData["IsEditable"] = isEditable;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public IActionResult Edit(UserProfileEditViewModel model)
        {
            if(model.DepartmentName==null) ModelState.AddModelError("Department", "Department cant be null");
            if (!AllowedDepartments.Contains(model.DepartmentName!))
            {
                ModelState.AddModelError("Department", "Invalid department selected.");
            }
            if (!AllowedPositions.Contains(model.Position!))
            {
                ModelState.AddModelError("Department", "Invalid position selected.");
            }
            var user = _userManager.Users.Include(u => u.Employee).ThenInclude(e=>e.Department)
                .FirstOrDefault(u => u.Id == model.Id);

            if (user == null) return NotFound();

            // Only allow current user or admin to update
            var currentUserId = _userManager.GetUserId(User);

            if (!User.IsInRole("manager") && !User.IsInRole("admin"))
            {
                _logger.LogInformation("EmployeeController.Edit called with invalid with no permissions user id: {UserId}", user.Id);
                return Forbid(); // 403 Forbidden
            }

            if (User.IsInRole("manager"))
            {
                //var currentUserId = _userManager.GetUserId(User);
                var currentUser = _userManager.Users
                    .Include(u => u.Employee).ThenInclude(e => e.Department)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser?.Employee?.Department != null && user.Employee?.Department != null)
                {
                    if (currentUser.Employee.Department != user.Employee.Department)
                    {
                        return Forbid();
                    }
                    else if(currentUser.Employee.Department == user.Employee.Department &&
                            user.Employee.Department.Name != model.DepartmentName )
                    {
                        return Forbid();
                    }
                }
            }
            if (ModelState.IsValid)
            { 
                //user.FirstName = model.FirstName;
                //user.LastName = model.LastName;
                if (user.Employee != null)
                {
                    user.Employee.Salary = model.Salary ?? user.Employee.Salary;
                    user.Employee.Position = model.Position ?? user.Employee.Position;
                    if (User.IsInRole("admin"))
                    {
                        var modelDepartment = _departmentRepository.GetDepartmentByName(user.Employee.Department.Name);
                        user.Employee.Department = modelDepartment ?? user.Employee.Department;
                    }
                        
                }
                _userManager.UpdateAsync(user).Wait();
                return RedirectToAction("List");
            }

            return RedirectToAction("Profile", new { userId = user.Id });

        }

        [HttpGet]
        [Authorize(Roles = "manager,admin")]
        public IActionResult HireEmployee(string userId)
        {
            var user = _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }
            
            if(user.Employee != null)
            {
                return Forbid();
            }

            var emp = new Employee
            {
                ApplicationUserId = userId
            };

            return View(emp);
        }

        [HttpPost]
        public IActionResult HireEmployee(Employee emp)
        {
            if (!AllowedDepartments.Contains(emp.Department.Name))
            {
                ModelState.AddModelError("Department", "Invalid department selected.");
            }
            if (!AllowedPositions.Contains(emp.Position))
            {
                ModelState.AddModelError("Department", "Invalid position selected.");
            }
            if (!ModelState.IsValid)
            {
                return View(emp);
            }
            var user = _userManager.FindByIdAsync(emp.ApplicationUserId).Result;
            if (user == null)
            {
                return NotFound("User not found");
            }
            if (user.Employee != null)
            {
                return Forbid();
            }
                         
            _employeeRepository.AddEmployee(emp);

            bool isInRole =  _userManager.IsInRoleAsync(user, "employee").Result;
            if (!isInRole)
            {
                var roleResult =  _userManager.AddToRoleAsync(user, "employee").Result;
                if (!roleResult.Succeeded)
                {
                    this._logger.LogError("EmployeeController.HireEmployee called with invalid with no permissions user id: {UserId}", user.Id);
                    ModelState.AddModelError("", "Failed to assign employee role.");
                    return View(emp);
                }
            }

            return RedirectToAction("Applicants");
            
            
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
