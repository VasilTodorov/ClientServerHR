using ClientServerHR.Models;
using ClientServerHR.Services;
using ClientServerHR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace ClientServerHR.Controllers
{
    //[Authorize(Roles = "employee")]
    public class EmployeeController : Controller
    {
        private readonly WorkingDaysService _service = new WorkingDaysService();        

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;
        
        private static readonly List<string> AllowedPositions = new()
        {
            "Junior Dev", "Mid Dev", "Senior dev", "Devops" 
        };

        public EmployeeController(IEmployeeRepository employeeRepository
                                , IDepartmentRepository departmentRepository
                                , UserManager<ApplicationUser> userManager
                                , ICountryRepository countryRepository
                                , ILogger<EmployeeController> logger)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _userManager = userManager;
            _countryRepository = countryRepository;
            _logger = logger;
            
        }
        #region Display               
        
        [Authorize(Roles = "manager,admin")]
        public IActionResult List(int? departmentId)
        {
            List<Employee> employees;
            var model = new DepartmentWithRoleViewModel();
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var user = _userManager.Users
                .Include(u => u.Employee)
                .ThenInclude(e=>e!.Department)
                .FirstOrDefault(u => u.Id == userId);

                if (user == null)
                    return NotFound();

                var roles = _userManager.GetRolesAsync(user!).Result;
                if(roles.Any(r=>r== "admin"))
                {
                    //employees = _employeeRepository.AllEmployees.ToList();
                    if(departmentId.HasValue)
                    {
                        var result = _departmentRepository.GetDepartmentById((int)departmentId);
                        if (result == null)
                            return NotFound();
                        model.Title = "Department: " + result.Name;
                        employees = _employeeRepository.AllEmployees.Where(e => e.DepartmentId == departmentId).ToList();
                    }
                    else
                    {
                        model.Title = "Employees";
                        employees = _employeeRepository.AllEmployees.ToList();
                    }
                    
                }
                else if(roles.Any(r => r == "manager"))
                {
                    model.Title= "Department: " + user.Employee?.Department?.Name ?? "";
                    employees = _employeeRepository.AllEmployees.Where(e => e.DepartmentId == user.Employee?.DepartmentId).ToList();
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                _logger.LogInformation("EmployeeController.List called with invalid user id: {UserId}", userId);
                return NotFound();
            }

            

            foreach (var emp in employees)
            {
                var roles = _userManager.GetRolesAsync(emp.ApplicationUser).Result;
                model.Employees.Add(new EmployeeWithRoleViewModel
                {
                    Employee = emp,
                    Roles = roles.ToList()
                });
            }
            model.DepartmentId = departmentId;            
                
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
        #endregion
        [Authorize(Roles = "manager,admin")]
        public IActionResult Delete(string userId, int? viewDepartmentId)
        {
            var user = _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (User.IsInRole("manager") && !User.IsInRole("admin"))
            {
                //var currentUserId = _userManager.GetUserId(User);
                var currentUser = _userManager.Users
                    .Include(u => u.Employee).ThenInclude(e => e!.Department)
                    .FirstOrDefault(u => u.Id == currentUserId);
                
                if(currentUser?.Employee?.DepartmentId == null)
                {
                    return Forbid();
                }
                else if (currentUser.Employee.DepartmentId != user.Employee?.DepartmentId)
                {
                    return Forbid();
                }                                    
            }

            if (user.Employee != null)            
            {
                _employeeRepository.Delete(user.Employee.EmployeeId);
                TempData["Message"] = "Employee deleted successfully.";

                var result = _userManager.DeleteAsync(user).Result;

                if (result.Succeeded)
                {
                    this._logger.LogInformation("EmployeeController.Delete {UserId} was deleted by {DeleterUserId}", userId, user.Id);
                    TempData["Message"] = "Employee deleted successfully.";
                    //return RedirectToAction("List"); // Or wherever you list users
                }else
                {
                    TempData["Message"] = "Employee deleted but user not.";
                }                
                return RedirectToAction("List", new { departmentId = viewDepartmentId });
            }
            
            var resultApplicant = _userManager.DeleteAsync(user).Result;
            if (resultApplicant.Succeeded)
            {
                TempData["Message"] = "Applicant deleted successfully.";
                return RedirectToAction("Applicants"); // Or wherever you list users
            }

            return RedirectToAction("Applicants");
            
        }
        [HttpPost]
        public IActionResult GetWorkingDays([FromForm] int month, [FromForm] int year)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users
                .Include(u => u.Employee).ThenInclude(e => e!.Country)
                .FirstOrDefault(u => u.Id == userId);

            if (user?.Employee?.Country == null)
                return Json(new { success = false });

            try
            {
                int workingDays;
                var monthName = new DateTime(year, month, 1)
                    .ToString("MMMM", CultureInfo.GetCultureInfo("en-US"));
                if (DateTime.Today.Year == year)
                {
                    workingDays = _service.GetWorkingDaysThisMonth(user.Employee.Country.Name, month);
                }
                else
                {
                    workingDays = _service.GetWorkingDaysThisMonth(user.Employee.Country.Name, month, year);
                }
                return Json(new 
                { 
                    success = true,
                    workingDays,
                    month,
                    year,
                    monthName
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch working days");
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult MyProfile()
        {
            if (User.Identity?.IsAuthenticated!=true)
            {
                return RedirectToAction("NotRegistered", "Employee");
            }
            var  userId = _userManager.GetUserId(User);

            ApplicationUser? user = _userManager.Users
                .Include(u => u.Employee).ThenInclude(e=>e!.Department)
                .Include(u => u.Employee).ThenInclude(e => e!.Country)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogInformation("EmployeeController.MyProfile called with invalid user id: {UserId}", userId);
                return NotFound();
            }
            
            var model = new MyProfileViewModel
            {
                User = user
            };
            if(user?.Employee?.Country != null)
            {
                model.IsEmployee= true;
                //model.User.Employee!.IBAN = _ibanProtector.DecryptIban(model.User.Employee.EncryptedIban);
                model.MonthWorkingDays = _service.GetWorkingDaysThisMonth(user.Employee.Country.Name, DateTime.Today.Month);
            }
            else
            {
                model.IsEmployee = false;
            }

            return View(model);
        }
        #region Edit        
        [Authorize(Roles = "manager,admin")]
        public IActionResult Edit(string? userId,int? viewDepartmentId)
        {
            var user = _userManager.Users
                .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return NotFound();
            if (user.Employee == null) return Forbid();

            // Check if current user can edit
            //bool isEditable = false;
            //if(User.IsInRole("manager") && !User.IsInRole("admin"))
            //{
            //    var currentUserId = _userManager.GetUserId(User);
            //    var currentUser = _userManager.Users
            //        .Include(u => u.Employee)
            //        .FirstOrDefault(u => u.Id == currentUserId);

            //    if(currentUser?.Employee?.Department != null && user.Employee?.Department!=null)
            //        isEditable = currentUser.Employee.Department == user.Employee.Department;
            //}
            //else if(User.IsInRole("admin"))
            //{
            //    isEditable = true;
            //}
            //if (!isEditable)
            //{
            //    _logger.LogInformation("EmployeeController.Profile called with invalid with no permissions user id: {UserId}", userId);
            //    return Forbid();
            //}


            var viewModel = new UserProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Position = user.Employee?.Position,
                Salary = user.Employee?.Salary,
                DepartmentName = user.Employee?.Department.Name,
                Departments = _departmentRepository.AllDepartments.ToList(),
                CountryOptions =  _countryRepository.CountryOptions,
                CountryId = user.Employee!.CountryId,
                ViewDepartmentId = viewDepartmentId,
                //IBAN = user.Employee?.IBAN
            };

            //ViewData["IsEditable"] = isEditable;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public IActionResult Edit(UserProfileEditViewModel model)
        {
            if(model.DepartmentName==null) ModelState.AddModelError("DepartmentName", "Department cant be null");
            else if (!_departmentRepository.AllDepartments.Select(d => d.Name).Contains(model.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "Invalid department selected.");
            }
            if(!_countryRepository.AllCountries.Select(c => c.CountryId).Contains(model.CountryId))
            {
                ModelState.AddModelError("CountryId", "Invalid country selected.");
            }
            if (!AllowedPositions.Contains(model.Position!))
            {
                ModelState.AddModelError("Position", "Invalid position selected.");
            }
            
            var user = _userManager.Users.Include(u => u.Employee).ThenInclude(e=>e!.Department)
                .FirstOrDefault(u => u.Id == model.Id);

            if (user == null) return NotFound();
            if (user.Employee == null) return Forbid();

            // Only allow current user or admin to update
            var currentUserId = _userManager.GetUserId(User);            

            if (User.IsInRole("manager") && !User.IsInRole("admin"))
            {
                //var currentUserId = _userManager.GetUserId(User);
                var currentUser = _userManager.Users
                    .Include(u => u.Employee).ThenInclude(e => e!.Department)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser?.Employee?.DepartmentId == null || user.Employee?.DepartmentId == null)
                {
                    return Forbid();
                }
                else if (currentUser.Employee.DepartmentId != user.Employee.DepartmentId)
                {
                    return Forbid();
                }
                else if(currentUser.Employee.DepartmentId == user.Employee.DepartmentId &&
                        user.Employee.Department.Name != model.DepartmentName )
                {
                    return Forbid();
                }
                
            }
            if (ModelState.IsValid)
            { 
                //user.FirstName = model.FirstName;
                //user.LastName = model.LastName;
                
                user.Employee.Salary = model.Salary ?? user.Employee.Salary;
                user.Employee.Position = model.Position ?? user.Employee.Position;
                user.Employee.CountryId = model.CountryId;
                if (User.IsInRole("admin"))
                {
                    var modelDepartment = _departmentRepository.GetDepartmentByName(model.DepartmentName!);
                    user.Employee.Department = modelDepartment ?? user.Employee.Department;
                }

                _employeeRepository.Update(user.Employee);
                //_userManager.UpdateAsync(user).Wait();                
                return RedirectToAction("List", new { departmentId=model.ViewDepartmentId });
            }

            return RedirectToAction("Edit", new { userId = user.Id, viewDepartmentId= model.ViewDepartmentId });

        }
        #endregion
        #region HireEmployee
        
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

            var emp = new HireEmployeeViewModel
            {
                ApplicationUserId = userId,
                Salary = 0m,
                Departments = _departmentRepository.AllDepartments.ToList(),
                CountryOptions = _countryRepository.CountryOptions.ToList()
            };
            return View(emp);
        }

        [HttpPost]
        [Authorize(Roles = "manager,admin")]
        public IActionResult HireEmployee(HireEmployeeViewModel emp)
        {
            //if (emp.DepartmentName == null) ModelState.AddModelError("DepartmentName", "Department Name cant be null");
            if (emp.DepartmentName != null && !_departmentRepository.AllDepartments.Select(d=>d.Name).Contains(emp.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "Invalid Department selected.");
            }
            if (emp.CountryId != null && !_countryRepository.AllCountries.Select(c => c.CountryId).Contains(emp.CountryId ?? 0))
            {
                ModelState.AddModelError("CountryId", "Invalid Country selected.");
            }
            if (emp.Position != null &&  !AllowedPositions.Contains(emp.Position ?? ""))
            {
                ModelState.AddModelError("DepartmentName", "Invalid Position selected.");
            }           
            
            if(ModelState.IsValid)
            {
                var user = _userManager.FindByIdAsync(emp.ApplicationUserId!).Result;
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (user.Employee != null)
                {
                    return Forbid();
                }
                var newEmployee = new Employee
                {
                    ApplicationUserId = emp.ApplicationUserId!,
                    Position = emp.Position!,
                    Salary = (decimal)emp.Salary!,
                    CountryId = emp.CountryId ?? 0,
                    Department = _departmentRepository.GetDepartmentByName(emp.DepartmentName!)!
                    
                };
                

                bool isInRole = _userManager.IsInRoleAsync(user, "employee").Result;
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

                _employeeRepository.AddEmployee(newEmployee);

                return RedirectToAction("Applicants");
            }
            else
            {
                emp.Departments = _departmentRepository.AllDepartments.ToList();
                emp.CountryOptions = _countryRepository.CountryOptions.ToList();
                return View(emp); 
            }
                                 
        }
        #endregion
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
