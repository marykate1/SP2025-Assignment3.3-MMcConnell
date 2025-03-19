using Microsoft.AspNetCore.Mvc;

namespace SP2025_Assignment3._3_MMcConnell.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
