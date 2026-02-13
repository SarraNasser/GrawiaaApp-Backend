using GrawiaaApp.API.Data;
using GrawiaaApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrawiaaApp.API.Services
{
    public class DailyLogClosureService : BackgroundService
    {
        private readonly ILogger<DailyLogClosureService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailyLogClosureService(
            ILogger<DailyLogClosureService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyLogClosureService started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // نفحص كل ساعة
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

                    await ProcessDailyClosure(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // طبيعي لما الخدمة تتوقف
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DailyLogClosureService at {Time}", DateTime.UtcNow);
                }
            }
        }

        private async Task ProcessDailyClosure(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // اليوم الحالي (بتوقيت UTC)
            var today = DateTime.UtcNow.Date;

            // إيجاد كل الـ logs اللي اتعملت النهاردة ومش مغلقة
            var openLogs = await context.DailyLogEntries
                .Where(l => l.LogDate.Date == today.AddDays(-1)) // أمس عشان نأكد الإغلاق
                .Where(l => l.DetectedEmotion == null) // اللي مش اتحللت NLP
                .ToListAsync(cancellationToken);

            foreach (var log in openLogs)
            {
                // وضع علامة "Finalized"
                log.IsHonest = true; // افتراضي لحد ما NLP يحلل
                log.SentimentScore = 0.5; // Neutral افتراضي

                // تسجيل الإغلاق
                _logger.LogInformation("Closed log {LogId} for child {ChildId}",
                    log.Id, log.ChildId);
            }

            if (openLogs.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Closed {Count} logs for date {Date}",
                    openLogs.Count, today.AddDays(-1));
            }
        }
    }
}
