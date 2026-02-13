
using GrawiaaApp.API.Data;
using GrawiaaApp.API.Interfaces;

namespace GrawiaaApp.API.Services
{
    public class GamingService : IGamingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<IScoreObserver> _observers;

        public GamingService(ApplicationDbContext context, IEnumerable<IScoreObserver> observers)
        {
            _context = context;
            _observers = observers;
        }

        public async Task<double> UpdateScore(int userId, int points)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return 0;

            user.TrustScore += points;

            // Record history for graph tracking in the mobile app
            _context.TrustScoreHistories.Add(new TrustScoreHistory
            {
                ChildId = userId,
                ScoreAtTimestamp = user.TrustScore,
                RecordedAt = DateTime.UtcNow,
                Reason = points > 0 ? "Task completed" : "System adjustment"
            });

            await _context.SaveChangesAsync();

            // Trigger observers (notifications & badges)
            if (_observers != null)
            {
                foreach (var observer in _observers)
                {
                    await observer.Execute(userId, user.TrustScore);
                }
            }

            return user.TrustScore;
        }
    }
}
