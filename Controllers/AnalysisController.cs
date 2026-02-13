using GrawiaaApp.API.Data;
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrawiaaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalysisController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// تحليل الـ Daily Log بالـ NLP (للـ NLP Team)
        /// </summary>
        [HttpPost("analyze-log/{logId}")]
        public async Task<IActionResult> AnalyzeLog(int logId, [FromBody] AnalyzeTextDto dto)
        {
            var log = await _context.DailyLogEntries.FindAsync(logId);
            if (log == null) return NotFound("Log not found");

            // حفظ نتايج الـ NLP (الـ NLP Team هتحلل النص)
            log.DetectedEmotion = dto.Emotion;      // "Joy", "Sadness", "Anger"
            log.SentimentScore = dto.Sentiment;     // 0.0 (negative) to 1.0 (positive)
            log.IsHonest = dto.IsHonest;           // Honesty detection

            // تحديث Trust Score بناءً على التحليل
            if (dto.IsHonest)
                log.Child.TrustScore = Math.Min(100, log.Child.TrustScore + 2);
            else
                log.Child.TrustScore = Math.Max(0, log.Child.TrustScore - 5);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Analysis saved successfully!",
                analysis = new
                {
                    emotion = dto.Emotion,
                    sentiment = dto.Sentiment,
                    honesty = dto.IsHonest,
                    trustImpact = dto.IsHonest ? "+2" : "-5"
                }
            });
        }

        /// <summary>
        /// الحصول على توصيات ذكية بناءً على نقاط الضعف
        /// </summary>
        [HttpGet("recommendations/{childId}")]
        [Authorize]
        public async Task<IActionResult> GetRecommendations(int childId)
        {
            var recentLogs = await _context.DailyLogEntries
                .Where(l => l.ChildId == childId && l.LogDate >= DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            var weakAreas = new List<string>();

            // تحليل نقاط الضعف البسيط (SRS 1.7)
            if (recentLogs.Count(l => string.IsNullOrEmpty(l.Sports)) > 3)
                weakAreas.Add("Sports");
            if (recentLogs.Count(l => string.IsNullOrEmpty(l.HomeworkStatus)) > 3)
                weakAreas.Add("Study");
            if (recentLogs.Average(l => l.SentimentScore) < 0.6)
                weakAreas.Add("Emotional");

            var recommendations = weakAreas.Select(area => new RecommendationDto
            {
                Category = area,
                Suggestion = area switch
                {
                    "Sports" => "Try playing football with friends 3x/week! ⚽",
                    "Study" => "Read 15 mins daily before bed 📚",
                    "Emotional" => "Write 3 things you're grateful for! 😊",
                    _ => "Keep up the great work! 🌟"
                }
            }).ToList();

            return Ok(new { recommendations });
        }

        /// <summary>
        /// تقرير أسبوعي للوالد (SRS 3.2)
        /// </summary>
        [Authorize(Roles = "Parent")]
        [HttpGet("weekly-report/{childId}")]
        public async Task<IActionResult> GetWeeklyReport(int childId)
        {
            var parentId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0");

            var child = await _context.Users.FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId);
            if (child == null) return Unauthorized();

            var weekLogs = await _context.DailyLogEntries
                .Where(l => l.ChildId == childId && l.LogDate >= DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            return Ok(new
            {
                childName = child.FullName,
                totalLogs = weekLogs.Count,
                avgSentiment = weekLogs.Average(l => (double?)l.SentimentScore) ?? 0.5,
                avgTrustScore = child.TrustScore,
                redFlags = weekLogs.Where(l => l.SentimentScore < 0.3).Count(),
                recommendations = await GetRecommendations(childId)
            });
        }
    }

    // DTOs للـ Analysis
    public class AnalyzeTextDto
    {
        public string Emotion { get; set; } = string.Empty;    // "Joy", "Sadness", "Anger"
        public double Sentiment { get; set; }                  // 0.0 to 1.0
        public bool IsHonest { get; set; }
        public string? DetectedText { get; set; }
    }

    public class RecommendationDto
    {
        public string Category { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
    }
}
