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
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly List<string> AllowedDepartments = new()
        {
            "HR", "IT", "Finance", "Sales", "Marketing"
        };
        private static readonly List<string> AllowedPositions = new()
        {
            "Junior Dev", "Mid Dev", "Senior dev", "Devops" 
        };

        public EmployeeController(IEmployeeRepository employeeRepository, UserManager<ApplicationUser> userManager)
        {
            _employeeRepository = employeeRepository;
            _userManager = userManager;
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
                return NotFound();
            }

            //roles = _userManager.GetRolesAsync(User).;
            //List<Employee> employees = _employeeRepository.AllEmployees.ToList();
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
        [Authorize(Roles = "manager,admin")]
        public IActionResult Delete(string? userId)
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
                    TempData["Message"] = "Employee deleted successfully.";
                    return RedirectToAction("List"); // Or wherever you list users
                }

                //TempData["Message"] = "Employee not deleted successfully.";
                return RedirectToAction("List");
            }
            
            var resultApplicant = _userManager.DeleteAsync(user).Result;
            if (resultApplicant.Succeeded)
            {
                TempData["Message"] = "Applicant deleted successfully.";
                return RedirectToAction("Applicants"); // Or wherever you list users
            }

            //TempData["Message"] = "Applicant not deleted successfully.";
            return RedirectToAction("Applicants");
            
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

            var viewModel = new UserProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Position = user.Employee?.Position,
                Salary = user.Employee?.Salary,
                Department = user.Employee?.Department
            };

            ViewData["IsEditable"] = isEditable;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public IActionResult Edit(UserProfileEditViewModel model)
        {
            if(model.Department==null) ModelState.AddModelError("Department", "Department cant be null");
            if (!AllowedDepartments.Contains(model.Department!))
            {
                ModelState.AddModelError("Department", "Invalid department selected.");
            }
            if (!AllowedPositions.Contains(model.Position!))
            {
                ModelState.AddModelError("Department", "Invalid position selected.");
            }
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
                            user.Employee.Department != model.Department )
                    {
                        return Forbid();
                    }
                }
            }
            if (ModelState.IsValid)
            { 
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                if (user.Employee != null)
                {
                    user.Employee.Salary = model.Salary ?? user.Employee.Salary;
                    user.Employee.Position = model.Position ?? user.Employee.Position;
                    if (User.IsInRole("admin"))
                        user.Employee.Department = model.Department ?? user.Employee.Department;
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
            //var user =  _userManager.FindByIdAsync(userId).Result;
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
            //return View();
        }
        [HttpPost]
        public IActionResult HireEmployee(Employee emp)
        {
            if (!AllowedDepartments.Contains(emp.Department))
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
                    // Optional: log or show error if needed
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
