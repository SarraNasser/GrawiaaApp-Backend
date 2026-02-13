using GrawiaaApp.API.Data;
using GrawiaaApp.API.Interfaces;
using GrawiaaApp.API.Models;

namespace GrawiaaApp.API.Logic
{
    public class ParentNotificationLogic : IScoreObserver
    {
        private readonly ApplicationDbContext _context;
        public ParentNotificationLogic(ApplicationDbContext context) => _context = context;

        public async Task Execute(int userId, double newScore)
        {
            var child = await _context.Users.FindAsync(userId);

            if (child != null && child.ParentId != null)
            {
                var notification = new Notification
                {
                    ReceiverId = child.ParentId.Value,
                    Message = $"Notification: {child.FullName}'s trust score is now {newScore}.",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
