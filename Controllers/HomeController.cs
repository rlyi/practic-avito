using Microsoft.AspNetCore.Mvc;

namespace AvitoClone.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}