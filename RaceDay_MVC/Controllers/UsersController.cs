using Microsoft.AspNetCore.Mvc;

namespace RaceDay_MVC.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
