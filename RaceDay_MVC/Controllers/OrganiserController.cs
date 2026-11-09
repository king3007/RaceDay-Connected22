using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
namespace RaceDay_MVC.Controllers;
[Authorize(Roles = "Organiser")]
public class OrganiserController(RaceDayApi api, LocalImageStore images, ILogger<OrganiserController> logger) : Controller {
 public async Task<IActionResult> OrganiserDashboard() => View(await api.Get<List<EventDto>>("api/events/mine"));
 [HttpGet] public IActionResult CreateEvent() => View(new CreateEventViewModel());
 [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
 public async Task<IActionResult> CreateEvent(CreateEventViewModel model) {
  ValidatedImage? banner = null;
  if (ModelState.IsValid) {
   try { banner = await images.ValidateAsync(model.BannerImage); }
   catch(ImageUploadException e) { ModelState.AddModelError(nameof(model.BannerImage),e.Message); }
  }
  if(!ModelState.IsValid) return View(model);
  System.Text.Json.JsonElement created;
  try { created = await api.Post<System.Text.Json.JsonElement>("api/events", new { model.Name, model.Description, model.EventDate }); }
  catch(ApiException e) { ModelState.AddModelError("",e.Message); return View(model); }
  TempData["Success"]="Event created.";
  if (banner != null) {
   if (!created.TryGetProperty("eventId", out var value) || !value.TryGetInt32(out var eventId) || eventId <= 0) {
    TempData["Error"]="Your event was created, but the API did not return its ID, so the banner could not be attached. Open the event overview to check it.";
   } else {
    try { await images.SaveAsync(ImageKind.Banners,eventId,banner); }
    catch(Exception e) when(e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) {
     logger.LogError(e,"Event {EventId} created but local banner save failed",eventId);
     TempData["Error"]="Your event was created, but the banner could not be saved. Check that the application can write to wwwroot/uploads and App_Data. You can retry the banner from the overview.";
    }
   }
  }
  return RedirectToAction(nameof(OrganiserDashboard));
 }
 [HttpGet] public async Task<IActionResult> Banner(int id) {
  var owned = await api.Get<List<EventDto>>("api/events/mine");
  var ev = owned.FirstOrDefault(e=>e.EventId==id); if(ev==null) return NotFound();
  return View(new EventBannerViewModel { EventId=id, EventName=ev.Name, BannerPath=images.GetPath(ImageKind.Banners,id) });
 }
 [HttpPost, RequestSizeLimit(6 * 1024 * 1024)] public async Task<IActionResult> Banner(int id, EventBannerViewModel model) {
  var owned = await api.Get<List<EventDto>>("api/events/mine");
  var ev = owned.FirstOrDefault(e=>e.EventId==id); if(ev==null) return NotFound();
  model.EventId=id; model.EventName=ev.Name; model.BannerPath=images.GetPath(ImageKind.Banners,id);
  if(model.BannerImage==null) ModelState.AddModelError(nameof(model.BannerImage),"Choose a banner image.");
  if(ModelState.IsValid) {
   try {
    var upload=await images.ValidateAsync(model.BannerImage);
    await images.SaveAsync(ImageKind.Banners,id,upload!);
    TempData["Success"]="Event banner updated."; return RedirectToAction(nameof(OrganiserDashboard));
   } catch(ImageUploadException e) { ModelState.AddModelError(nameof(model.BannerImage),e.Message); }
   catch(Exception e) when(e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) {
    logger.LogError(e,"Could not save banner for {EventId}",id);
    ModelState.AddModelError("","Could not save the banner. Check write permissions for wwwroot/uploads and App_Data.");
   }
  }
  return View(model);
 }
 public async Task<IActionResult> Categories(int id) {
  var events = await api.Get<List<EventDto>>("api/events/mine");
  var ev = events.FirstOrDefault(e => e.EventId == id); if(ev == null) return NotFound();
  ViewBag.EventId=id; ViewBag.EventName=ev.Name; ViewBag.Categories=ev.Categories;
  return View(new AddCategoryModel());
 }
 [HttpPost] public async Task<IActionResult> Categories(int id, AddCategoryModel model) {
  if(!ModelState.IsValid) { TempData["Error"]="Enter a category name (up to 100 characters) and type (up to 20)."; return RedirectToAction(nameof(Categories),new{id}); }
  await api.Post<System.Text.Json.JsonElement>($"api/events/{id}/categories",model);
  TempData["Success"]="Category added."; return RedirectToAction(nameof(Categories),new{id});
 }
 public async Task<IActionResult> Enrolments(int id) { ViewBag.EventId=id; return View(await api.Get<List<EnrolmentDto>>($"api/events/{id}/enrolments")); }
 public async Task<IActionResult> Results(int id) { ViewBag.EventId=id; return View(await api.Get<List<ResultDto>>($"api/events/{id}/results")); }
 [HttpPost] public async Task<IActionResult> CaptureResult(int id, CaptureResultModel model) {
  if(!ModelState.IsValid || model.FinishTime < TimeSpan.Zero) { TempData["Error"]="Enter a valid enrolment, non-negative finish time (hh:mm:ss), and position."; return RedirectToAction(nameof(Enrolments),new{id}); }
  try { await api.Post<System.Text.Json.JsonElement>($"api/events/{id}/results",model); TempData["Success"]="Result saved."; }
  catch(ApiException e) when((int)e.Status == 400 || (int)e.Status == 409) { TempData["Error"]=e.Message; }
  return RedirectToAction(nameof(Enrolments),new{id});
 }
}
