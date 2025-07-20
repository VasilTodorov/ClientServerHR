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
            var employees = _employeeRepository.AllEmployees.ToList();
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

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            if (user.Employee != null)
            {
                user.Employee.Salary = model.Salary ?? user.Employee.Salary;
                if (User.IsInRole("admin"))
                    user.Employee.Department = model.Department ?? user.Employee.Department;
            }

            _userManager.UpdateAsync(user).Wait();
            return RedirectToAction("Profile", new { userId = user.Id });
        }

        [HttpGet]
        public IActionResult HireEmployee(string userId)
        {
            var user =  _userManager.FindByIdAsync(userId).Result;
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Prevent hiring if already an employee
            //var alreadyEmployee = _employeeRepository.AllEmployees.Any(e => e.ApplicationUserId == userId);
            //if (alreadyEmployee)
            //{
            //    return RedirectToAction("AlreadyHired");
            //}
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
