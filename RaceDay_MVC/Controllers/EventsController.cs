using Microsoft.AspNetCore.Mvc;
namespace RaceDay_MVC.Controllers;
public class EventsController : Controller { public IActionResult Index() => RedirectToAction("Events","Home"); }
