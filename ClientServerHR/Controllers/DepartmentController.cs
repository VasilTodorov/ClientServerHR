using ClientServerHR.Models;
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
    }
}
