using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_POE.Models;
using System.Security.Claims;

namespace RACEDAY_API_POE.Data
{
    [Route("api/events/{eventId}/categories")]
    [ApiController]
    [Authorize(Roles = "Organiser")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;

        public CategoriesController(RaceDayDatabaseContext context)
        {
            _context = context;
        }

        public class CategoryDto { public string Name { get; set; } = string.Empty; public string CategoryType { get; set; } = string.Empty; }

        // POST: api/events/{eventId}/categories
        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, [FromBody] CategoryDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null || ev.OrganiserId != organiser!.OrganiserId)
                return StatusCode(403, new { message = "Not your event." });

            var category = new Category { EventId = eventId, Name = dto.Name, CategoryType = dto.CategoryType };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Category added.", categoryId = category.CategoryId });
        }
    }
}
