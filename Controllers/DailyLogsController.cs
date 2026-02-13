using GrawiaaApp.API.Data;
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrawiaaApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DailyLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DailyLogsController(ApplicationDbContext context) => _context = context;

        /// <summary>
        /// View a child's daily logs (Parent only)
        /// </summary>
        [Authorize(Roles = "Parent")]
        [HttpGet("child-logs/{childId}")]
        public async Task<IActionResult> GetChildLogs(int childId)
        {
            var parentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var isRelated = await _context.Users.AnyAsync(u => u.Id == childId && u.ParentId == parentId);
            if (!isRelated) return Unauthorized("You do not have permission to view this child's logs.");

            var logs = await _context.DailyLogEntries
                .Where(l => l.ChildId == childId)
                .Include(l => l.Feedbacks)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

            return Ok(logs);
        }
    }
}
