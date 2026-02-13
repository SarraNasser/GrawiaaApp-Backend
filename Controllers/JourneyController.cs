using GrawiaaApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GrawiaaApp.API.Models;

namespace GrawiaaApp.API.Controllers
{
    [Authorize(Roles = "Child")]
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JourneyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("progress")]
        public async Task<IActionResult> GetJourneyProgress()
        {
            var childId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var totalLogs = await _context.DailyLogEntries
                .Where(l => l.ChildId == childId)
                .GroupBy(l => EF.Functions.DateFromParts(l.LogDate.Year, l.LogDate.Month, l.LogDate.Day))
                .CountAsync(); // عدد الأيام الفريدة

            var totalBadges = await _context.Badges
                .CountAsync(b => b.ChildId == childId);

            var trustScore = await _context.Users
                .Where(u => u.Id == childId)
                .Select(u => u.TrustScore)
                .FirstOrDefaultAsync();

            var completedTasks = await _context.ChildTasks
                .CountAsync(t => t.ChildId == childId && t.IsCompleted);

            // حساب المستوى
            var currentLevel = Math.Min(20, (totalLogs / 3) + (totalBadges * 2) + (completedTasks / 5) + 1);
            var progressPercentage = Math.Min(100, currentLevel * 5);

            return Ok(new
            {
                currentLevel,
                progressPercentage,
                totalLogs,
                totalBadges,
                trustScore,
                completedTasks,
                nextMilestone = currentLevel switch
                {
                    <= 3 => "Log 5 more days! 📖",
                    <= 7 => "Complete 3 tasks! ✅",
                    <= 12 => "Reach 200 Trust Score! ⭐",
                    <= 18 => "Unlock 5 badges! 🏆",
                    _ => "Grawiaa Hero! 🌟"
                }
            });
        }

        [HttpGet("milestones")]
        public async Task<IActionResult> GetMilestones()
        {
            var childId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var badges = await _context.Badges
                .Where(b => b.ChildId == childId)
                .OrderBy(b => b.UnlockedAt)
                .Select(b => new
                {
                    b.Name,
                    b.Description,
                    b.IconUrl,
                    b.UnlockedAt,
                    MilestoneType = b.Name.Contains("Explorer") ? "Engagement" :
                                   b.Name.Contains("Hero") ? "Trust" : "Consistency"
                })
                .ToListAsync();

            return Ok(new { milestones = badges });
        }
    }
}
