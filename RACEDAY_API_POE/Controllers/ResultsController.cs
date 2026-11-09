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
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;

        public ResultsController(RaceDayDatabaseContext context)
        {
            _context = context;
        }
        // method to get the current user's ID from the JWT claims
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // POST: api/events/{eventId}/results (Organiser Only)
        [HttpPost("events/{eventId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CaptureResult(int eventId, [FromBody] ResultDto dto)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null || ev.OrganiserId != organiser!.OrganiserId)
                return StatusCode(403, new { message = "Not authorized for this event." });

            // Verify the enrolment belongs to this event
            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.UniqueId == dto.EnrolmentId && e.EventId == eventId);

            if (enrolment == null) return BadRequest(new { message = "Invalid enrolment ID for this event." });

            // Check if result already exists
            if (await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
                return Conflict(new { message = "Result for this enrolment already exists." });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                FinishingPosition = dto.FinishingPosition
            };

            _context.Results.Add(result);

            // Optionally update enrolment status to "Completed"
            enrolment.Status = "Completed";

            await _context.SaveChangesAsync();
            return StatusCode(201, new { message = "Result captured successfully." });
        }

        // GET: api/users/results (Participant Only)
        [HttpGet("users/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetUserId();
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null) return Unauthorized();

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.ParticipantId == participant.ParticipantId)
                .Select(r => new {
                    r.ResultId,
                    EventName = r.Enrolment.Event.Name,
                    CategoryName = r.Enrolment.Category.Name,
                    r.FinishTime,
                    r.FinishingPosition
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/events/{eventId}/results (Organiser Only)
        [HttpGet("events/{eventId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser!.OrganiserId) return StatusCode(403, new { message = "Not your event." });

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.EventId == eventId)
                .Select(r => new {
                    r.ResultId,
                    ParticipantName = $"{r.Enrolment.Participant.User.FirstName} {r.Enrolment.Participant.User.LastName}",
                    CategoryName = r.Enrolment.Category.Name,
                    r.FinishTime,
                    r.FinishingPosition
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
