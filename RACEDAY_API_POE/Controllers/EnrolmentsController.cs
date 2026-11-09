using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_POE.Data;
using RACEDAY_API_POE.Models;
using System.Security.Claims;
using RACEDAY_API_POE.DTOs;

namespace RACEDAY_API_POE.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;

        public EnrolmentsController(RaceDayDatabaseContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // POST: api/events/{eventId}/enrol (Participant Only)
        [HttpPost("events/{eventId}/enrol")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> EnrolInEvent(int eventId, [FromBody] EnrolmentDto dto)
        {
            var userId = GetUserId();
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null) return Unauthorized();

            if (!await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId))
                return BadRequest(new { message = "Select a category belonging to this event." });

            // Check if already enrolled
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == participant.ParticipantId &&
                               e.EventId == eventId &&
                               e.CategoryId == dto.CategoryId);

            if (alreadyEnrolled) return Conflict(new { message = "You are already enrolled in this category." });

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = eventId,
                CategoryId = dto.CategoryId,
                Status = "Registered"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Successfully enrolled.", enrolmentId = enrolment.UniqueId });
        }

        // GET: api/users/enrolments (Participant Only)
        [HttpGet("users/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetUserId();
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null) return Unauthorized();

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .Select(e => new {
                    e.UniqueId,
                    e.Status,
                    EventName = e.Event.Name,
                    EventDate = e.Event.EventDate,
                    CategoryName = e.Category.Name
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/{eventId}/enrolments (Organiser Only)
        [HttpGet("events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != organiser!.OrganiserId) return StatusCode(403, new { message = "Not your event." });

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new {
                    e.UniqueId,
                    ParticipantName = $"{e.Participant.User.FirstName} {e.Participant.User.LastName}",
                    CategoryName = e.Category.Name,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
