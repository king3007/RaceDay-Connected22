using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_POE.Data;
using RACEDAY_API_POE.DTOs;
using RACEDAY_API_POE.Models;
using System.Security.Claims;

namespace RACEDAY_API_POE.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;

        public EventsController(RaceDayDatabaseContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // GET: api/events (Everyone)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .Select(e => new {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    OrganiserName = e.Organiser.CompanyName,
                    Categories = e.Categories.Select(c => new { c.CategoryId, c.Name, c.CategoryType })
                })
                .ToListAsync();

            return Ok(events);
        }

        // Lists only events owned by the signed-in organiser for the MVC dashboard.
        [HttpGet("mine")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetUserId();
            var events = await _context.Events
                .Where(e => e.Organiser.UserId == userId)
                .Select(e => new {
                    e.EventId, e.Name, e.Description, e.EventDate,
                    OrganiserName = e.Organiser.CompanyName,
                    Categories = e.Categories.Select(c => new { c.CategoryId, c.Name, c.CategoryType })
                }).ToListAsync();
            return Ok(events);
        }

        // GET: api/events/{id} (Everyone)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

        // POST: api/events (Organiser Only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null) return Unauthorized(new { message = "Organiser profile not found." });

            var newEvent = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                EventDate = dto.EventDate
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Event created.", eventId = newEvent.EventId });
        }

        // PUT: api/events/{id} (Organiser Only - can only edit their own)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != organiser!.OrganiserId) return Forbidden(); // Not their event!

            ev.Name = dto.Name.Trim();
            ev.Description = dto.Description?.Trim();
            ev.EventDate = dto.EventDate;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated." });
        }

        // DELETE: api/events/{id} (Organiser Only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != organiser!.OrganiserId) return Forbidden();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        //restricted access message for unauthorised users trying to modify events
        private IActionResult Forbidden() => StatusCode(403, new { message = "You do not have permission to modify this event." });
    }
}
