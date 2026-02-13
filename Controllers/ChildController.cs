
using GrawiaaApp.API.Data;
using GrawiaaApp.API.DTOs;
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrawiaaApp.API.Controllers
{
    [Authorize(Roles = "Child")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChildrenController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// (Image 2) Quick Auto-save for Water and Mood
        /// </summary>
        [HttpPatch("auto-save-log")]
        public async Task<IActionResult> AutoSaveLog([FromBody] AutoSaveLogDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var today = DateTime.UtcNow.Date;

            var log = await _context.DailyLogEntries
                .FirstOrDefaultAsync(l => l.ChildId == userId && l.LogDate.Date == today);

            if (log == null)
            {
                log = new DailyLogEntry { ChildId = userId, LogDate = DateTime.UtcNow };
                _context.DailyLogEntries.Add(log);
            }

            if (dto.WaterCups > 0) log.WaterCups = dto.WaterCups;
            if (!string.IsNullOrEmpty(dto.MoodEmoji)) log.MoodEmoji = dto.MoodEmoji;

            if (!string.IsNullOrEmpty(dto.Note))
            {
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(dto.Note);
                log.Note = System.Convert.ToBase64String(textBytes);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Quick update saved! ⚡" });
        }

        /// <summary>
        /// (Image 1 & 6) Get child's profile and badges
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var child = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new ChildProfileDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    TrustScore = u.TrustScore,
                    CharacterName = u.CharacterName,
                    CharacterType = u.CharacterType,
                    IsLinked = u.ParentId != null,
                    Badges = _context.Badges
                        .Where(b => b.ChildId == userId)
                        .Select(b => new BadgeDto
                        {
                            Name = b.Name,
                            Description = b.Description,
                            IconUrl = b.IconUrl,
                            UnlockedAt = b.UnlockedAt
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (child == null) return NotFound(new { message = "Child profile not found." });
            return Ok(child);
        }

        /// <summary>
        /// (Image 4) Submit full daily journal with text descriptions
        /// </summary>
        [HttpPost("submit-log")]
        public async Task<IActionResult> SubmitDailyLog([FromBody] CreateDailyLogDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var today = DateTime.UtcNow.Date;

            var log = await _context.DailyLogEntries
                .FirstOrDefaultAsync(l => l.ChildId == userId && l.LogDate.Date == today);

            if (log == null)
            {
                log = new DailyLogEntry { ChildId = userId, LogDate = DateTime.UtcNow };
                _context.DailyLogEntries.Add(log);
            }

            // ربط الحقول من وصفك للشاشة بالموديل
            log.MoodEmoji = dto.MoodEmoji;          // How are you feeling?
            log.FoodFruitsVeggies = dto.FoodNote;   // What did you eat?
            log.Sports = dto.ActivityNote;          // Physical activity
            log.HomeworkStatus = dto.LearningNote;  // What did you learn?
            log.Note = dto.SleepNote;               // How did you sleep? (Encrypted or raw based on your Model)
            log.WaterCups = dto.WaterCups;          // Water intake

            await _context.SaveChangesAsync();

            // نظام الأوسمة (Badge System)
            var logsCount = await _context.DailyLogEntries.CountAsync(l => l.ChildId == userId);
            if (logsCount == 1 && !await _context.Badges.AnyAsync(b => b.ChildId == userId && b.Name == "First Hero"))
            {
                _context.Badges.Add(new Badge
                {
                    ChildId = userId,
                    Name = "First Hero",
                    Description = "Your first journal entry! 📖",
                    IconUrl = "hero1.png",
                    UnlockedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Save My Day! Your journal is locked in the treasure chest 🏴‍☠️✨" });
        }

        /// <summary>
        /// (Image 3) Submit answer to a daily question (Independent Model)
        /// </summary>
        [HttpPost("submit-question-answer")]
        public async Task<IActionResult> SubmitQuestionAnswer([FromBody] DailyQuestionAnswerDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var questionEntry = new DailyQuestionAnswer
            {
                ChildId = userId,
                Answer = dto.Answer,
                DateAnswered = DateTime.UtcNow
            };

            _context.DailyQuestionAnswers.Add(questionEntry);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Your thought has been saved! 🌟" });
        }

        /// <summary>
        /// (Image 7) Get weekly statistics for charts
        /// </summary>
        [HttpGet("my-statistics")]
        public async Task<IActionResult> GetStats()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var last7DaysLogs = await _context.DailyLogEntries
                .Where(l => l.ChildId == userId)
                .OrderByDescending(l => l.LogDate)
                .Take(7)
                .ToListAsync();

            if (!last7DaysLogs.Any())
                return Ok(new { message = "Start logging to see your stats!" });

            var total = (float)last7DaysLogs.Count;
            var stats = new StatsResponseDto
            {
                WeeklyLogs = last7DaysLogs.Select(l => new DailyLogSummaryDto
                {
                    Date = l.LogDate.ToString("ddd"),
                    Water = l.WaterCups,
                    Mood = l.MoodEmoji
                }).ToList(),
                PieChartData = new CategoryDistribution
                {
                    StudyPercentage = (last7DaysLogs.Count(l => !string.IsNullOrEmpty(l.HomeworkStatus)) / total) * 100,
                    SportsPercentage = (last7DaysLogs.Count(l => !string.IsNullOrEmpty(l.Sports)) / total) * 100,
                    FoodPercentage = (last7DaysLogs.Count(l => !string.IsNullOrEmpty(l.FoodFruitsVeggies)) / total) * 100
                }
            };

            return Ok(stats);
        }

        // ... باقي الميثودز زي GetProfile و SubmitDailyLog ...

        /// <summary>
        /// Interactive tip from the child's chosen character based on their status
        /// </summary>
        [HttpGet("character-tip")]
        public async Task<IActionResult> GetCharacterTip()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var child = await _context.Users.FindAsync(userId);
            string charName = child?.CharacterName ?? "Your Friend";

            var lastLog = await _context.DailyLogEntries
                .Where(l => l.ChildId == userId)
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefaultAsync();

            string tip = $"{charName} is so proud of you, Hero! 🌟";

            if (lastLog != null)
            {
                if (lastLog.WaterCups < 3)
                    tip = $"{charName} thinks your body needs energy, drink water to keep {charName} shining! 💧";
                else if (lastLog.MoodEmoji == "Sad")
                    tip = $"{charName} feels you.. remember that you are strong and loved! ❤️";
            }

            return Ok(new { tip });
        }


      
        [HttpGet("daily-question")]
        public async Task<IActionResult> GetDailyQuestion()
        {
            var questions = new[] { "What made you smile today?", "Who did you help today?" };
            return Ok(new { question = questions[new Random().Next(questions.Length)] });
        }

        [HttpPost("send-message-to-parent")]
        public async Task<IActionResult> SendMessage([FromBody] ChildMessageDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var child = await _context.Users.FindAsync(userId);

            if (child?.ParentId == null) return BadRequest("Link your parent first.");

            _context.Notifications.Add(new Notification
            {
                ReceiverId = child.ParentId.Value,
                Message = $"Message from {child.FullName}: {dto.Content}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sent to parent! 💌" });
        }


        [Authorize(Roles = "Child")]
        [HttpPost("confirm-parent-link")]
        public async Task<IActionResult> ConfirmParentLink([FromBody] ConfirmLinkDto dto)
        {
            var childId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var child = await _context.Users.FindAsync(childId);

            var linkRequest = await _context.ParentLinkRequests
                .FirstOrDefaultAsync(r => r.ChildId == childId && r.Token == dto.Token);

            if (linkRequest == null || linkRequest.IsUsed || linkRequest.ExpiryDate < DateTime.UtcNow)
                return BadRequest("Invalid or expired link token");

            // ربط الطفل بالأب
            child.ParentId = linkRequest.ChildId; // أو parent ID من الـ request
            linkRequest.IsUsed = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Parent linked successfully! 🎉" });
        }

    }
}