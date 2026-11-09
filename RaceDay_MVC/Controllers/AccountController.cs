using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
namespace RaceDay_MVC.Controllers;
[Authorize]
public class AccountController(RaceDayApi api, LocalImageStore images, ILogger<AccountController> logger) : Controller
{
 private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 [HttpGet] public async Task<IActionResult> Profile() {
  var current=await api.Get<UserProfileDto>("api/users/profile");
  return View(new ProfileViewModel { FirstName=current.FirstName, LastName=current.LastName, Email=current.Email, Role=current.Role, PhoneNumber=current.PhoneNumber, PhotoPath=images.GetPath(ImageKind.Profiles,UserId) });
 }
 [HttpPost, RequestSizeLimit(6 * 1024 * 1024)] public async Task<IActionResult> Profile(ProfileViewModel model) {
  var current=await api.Get<UserProfileDto>("api/users/profile");
  model.Email=current.Email; model.Role=current.Role; model.PhotoPath=images.GetPath(ImageKind.Profiles,UserId);
  ValidatedImage? photo=null;
  if(ModelState.IsValid) {
   try { photo=await images.ValidateAsync(model.ProfileImage); }
   catch(ImageUploadException e) { ModelState.AddModelError(nameof(model.ProfileImage),e.Message); }
  }
  if(!ModelState.IsValid) return View(model);
  try { await api.Put("api/users/profile", new { model.FirstName, model.LastName, PhoneNumber=User.IsInRole("Organiser") ? model.PhoneNumber : current.PhoneNumber }); }
  catch(ApiException e) { ModelState.AddModelError("",e.Message); return View(model); }
  if(photo!=null) {
   try { await images.SaveAsync(ImageKind.Profiles,UserId,photo); }
   catch(Exception e) when(e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) {
    logger.LogError(e,"Profile updated but image save failed for user {UserId}",UserId);
    ModelState.AddModelError("","Your account details were saved, but the photo could not be saved. Check write permissions for wwwroot/uploads and App_Data, then select the photo again.");
    return View(model);
   }
  }
  // Refresh the displayed name while retaining the original API token and session expiry.
  var session=await HttpContext.AuthenticateAsync();
  if(session.Succeeded && session.Principal?.Identity is ClaimsIdentity identity) {
   foreach(var claim in identity.FindAll(ClaimTypes.Name).ToList()) identity.RemoveClaim(claim);
   identity.AddClaim(new Claim(ClaimTypes.Name,$"{model.FirstName.Trim()} {model.LastName.Trim()}"));
   await HttpContext.SignInAsync(session.Principal,session.Properties);
  }
  TempData["Success"]="Profile updated."; return RedirectToAction(nameof(Profile));
 }
}
