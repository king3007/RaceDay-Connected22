using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
namespace RaceDay_MVC.Controllers;
[Authorize(Roles = "Participant")]
public class ParticipantController(RaceDayApi api) : Controller {
 public async Task<IActionResult> ParticipantDashboard() => View(new ParticipantDashboardModel { Enrolments = await api.Get<List<EnrolmentDto>>("api/users/enrolments"), Results = await api.Get<List<ResultDto>>("api/users/results") });
 public async Task<IActionResult> BrowseEvents() => View(await api.Get<List<EventDto>>("api/events"));
 public async Task<IActionResult> MyEnrolments() => View(await api.Get<List<EnrolmentDto>>("api/users/enrolments"));
 public async Task<IActionResult> MyResults() => View(await api.Get<List<ResultDto>>("api/users/results"));
 public IActionResult Profile() => RedirectToAction("Profile","Account");
 [HttpPost] public async Task<IActionResult> Enrol(int eventId,int categoryId) {
  if(eventId <= 0 || categoryId <= 0) return BadRequest();
  var events = await api.Get<List<EventDto>>("api/events");
  if(!events.Any(e=>e.EventId==eventId && e.Categories.Any(c=>c.CategoryId==categoryId))) return BadRequest("Select a category belonging to this event.");
  try { await api.Post<System.Text.Json.JsonElement>($"api/events/{eventId}/enrol",new { CategoryId=categoryId }); TempData["Success"]="You are enrolled."; return RedirectToAction(nameof(MyEnrolments)); }
  catch(ApiException e) when((int)e.Status == 409 || (int)e.Status == 400) { TempData["Error"]=e.Message; return RedirectToAction(nameof(BrowseEvents)); }
 }
}
