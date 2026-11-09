using Microsoft.AspNetCore.Mvc;

namespace RaceDay_MVC.Controllers
{
    public class RaceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
