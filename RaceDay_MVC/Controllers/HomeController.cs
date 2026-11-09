using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Security.Claims;
namespace RaceDay_MVC.Controllers;
public class HomeController(RaceDayApi api) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
    public IActionResult AboutUs() => View();
    public IActionResult ContactUs() => View();
    public async Task<IActionResult> Calendar() => View(await api.Get<List<EventDto>>("api/events"));
    public IActionResult Results() => View();
    public async Task<IActionResult> Events() => View(await api.Get<List<EventDto>>("api/events"));
    public IActionResult AccessDenied() { Response.StatusCode = 403; ViewData["Message"] = "This page is for a different account role."; return View("~/Views/Shared/ApiError.cshtml"); }
    [HttpGet] public IActionResult Register() => View(new RegisterDto());
    [HttpPost] public async Task<IActionResult> Register(RegisterDto model) {
        if (!ModelState.IsValid) return View(model);
        try {
            await api.Post<System.Text.Json.JsonElement>("api/auth/register", new {
                model.FirstName, model.LastName, model.Email, model.Password,
                PhoneNumber = model.PhoneNumber ?? "", model.Role, model.DateOfBirth, model.Gender
            });
            TempData["Success"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        } catch (ApiException e) { ModelState.AddModelError("", e.Message); return View(model); }
    }
    [HttpGet] public IActionResult Login() => View(new LoginDto());
    [HttpPost] public async Task<IActionResult> Login(LoginDto model) {
        if (!ModelState.IsValid) return View(model);
        try {
            var result = await api.Post<LoginResponseDto>("api/auth/login", model);
            if (string.IsNullOrWhiteSpace(result.Token) || result.ExpiresInMinutes <= 0 || (result.User.Role != "Organiser" && result.User.Role != "Participant")) {
                ModelState.AddModelError("", "The API returned an incomplete login response."); return View(model);
            }
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier,result.User.UserId.ToString()), new Claim(ClaimTypes.Name,result.User.FirstName + " " + result.User.LastName), new Claim(ClaimTypes.Role,result.User.Role) };
            var properties = new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(result.ExpiresInMinutes) };
            properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = result.Token } });
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), properties);
            Response.Cookies.Delete("JwtToken"); Response.Cookies.Delete("UserRole");
            return result.User.Role == "Organiser" ? RedirectToAction("OrganiserDashboard", "Organiser") : RedirectToAction("ParticipantDashboard", "Participant");
        } catch (ApiException e) { ModelState.AddModelError("", e.Message); return View(model); }
    }
    [HttpPost] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return RedirectToAction(nameof(Index)); }
    public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
