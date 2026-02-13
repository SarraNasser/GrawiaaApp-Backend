using GrawiaaApp.API.Data;
using GrawiaaApp.API.DTOs;
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrawiaaApp.API.Controllers
{
    [Authorize(Roles = "Parent")]
    [Route("api/[controller]")]
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ParentController(ApplicationDbContext context) => _context = context;

        

        /// <summary>
        /// Force child to reset password
        /// </summary>
        [HttpPost("reset-child-password/{childId}")]
        public async Task<IActionResult> ResetChildPassword(int childId)
        {
            var parentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var child = await _context.Users.FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId);

            if (child == null) return NotFound("Child not found or not linked to your account.");

            child.PasswordResetRequired = true; // Enforced at child's login
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset request activated. The child will be prompted at next login." });
        }

        /// <summary>
        /// Parent dashboard for a child
        /// </summary>
        [HttpGet("child-dashboard/{childId}")]
        public async Task<IActionResult> GetChildDashboardForParent(int childId)
        {
            var parentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var child = await _context.Users
                .Include(u => u.Logs)
                .Include(u => u.Badges)
                .FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId);

            if (child == null) return Unauthorized("This child is not linked to your account.");

            return Ok(new
            {
                FullName = child.FullName,
                CurrentScore = child.TrustScore,
                Character = child.CharacterType,
                RecentLogs = child.Logs.OrderByDescending(l => l.LogDate).Take(7).Select(l => new {
                    l.LogDate,
                    l.MoodEmoji,
                    l.WaterCups,
                    l.HomeworkStatus
                }),
                Badges = child.Badges.Select(b => new { b.Name, b.IconUrl, b.UnlockedAt })
            });
        }

        /// <summary>
        /// Add encouragement/feedback to child
        /// </summary>
        [HttpPost("add-feedback")]
        public async Task<IActionResult> AddFeedback([FromBody] ParentFeedbackDto dto)
        {
            var log = await _context.DailyLogEntries.FindAsync(dto.LogId);
            if (log == null) return NotFound("Daily log not found.");

            var feedback = new ParentFeedback
            {
                DailyLogEntryId = dto.LogId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParentFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback successfully sent to the child! 🌟" });
        }

        /// <summary>
        /// View messages sent by child
        /// </summary>
        [HttpGet("messages-from-child/{childId}")]
        public async Task<IActionResult> GetMessages(int childId)
        {
            var parentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var messages = await _context.Notifications
                .Where(n => n.ReceiverId == parentId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        /// <summary>
        /// Generate child link token
        /// </summary>
        [HttpPost("generate-link-token/{childId}")]
        public async Task<IActionResult> GenerateLinkToken(int childId)
        {
            var parentEmail = User.FindFirst(ClaimTypes.Email)?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var token = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var linkRequest = new ParentLinkRequest
            {
                ChildId = childId,
                ParentEmail = parentEmail ?? "Not Provided",
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            _context.ParentLinkRequests.Add(linkRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Link token generated. Give it to the child to enter in their account.", token });
        }
    }
}
