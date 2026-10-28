using System.Security.Claims;
using CampusFix.Data;
using CampusFix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers.Api
{
    [Route("api/maintenance-requests")]
    [ApiController]
    [Authorize]
    public class MaintenanceRequestsApiController : ControllerBase
    {
        private readonly CampusFixDbContext _context;

        public MaintenanceRequestsApiController(CampusFixDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.MaintenanceRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .AsQueryable();

            if (!User.IsInRole("Maintenance") && !User.IsInRole("Administrator"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
                query = query.Where(r => r.UserId == userId);
            }

            var requests = await query
                .OrderByDescending(r => r.DateCreated)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Location,
                    r.DateCreated,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    Category = r.Category == null ? null : r.Category.Name
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _context.MaintenanceRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            if (!User.IsInRole("Maintenance") && !User.IsInRole("Administrator"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
                if (request.UserId != userId) return Forbid();
            }

            return Ok(new
            {
                request.Id,
                request.Title,
                request.Description,
                request.Location,
                request.DateCreated,
                Status = request.Status.ToString(),
                Priority = request.Priority.ToString(),
                Category = request.Category?.Name
            });
        }
    }
}
