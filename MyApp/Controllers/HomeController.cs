using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult FactoryDashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            // 直接導向 FactoryDashboard
            return RedirectToAction("FactoryDashboard");
        }
    }
}
