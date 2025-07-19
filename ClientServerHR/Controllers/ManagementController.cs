using Microsoft.AspNetCore.Mvc;

namespace ClientServerHR.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
