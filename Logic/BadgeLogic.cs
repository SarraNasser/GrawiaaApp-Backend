
using GrawiaaApp.API.Data;
using GrawiaaApp.API.Interfaces;
using GrawiaaApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GrawiaaApp.API.Logic
{
    public class BadgeLogic : IScoreObserver
    {
        private readonly ApplicationDbContext _context;
        public BadgeLogic(ApplicationDbContext context) => _context = context;

        public async Task Execute(int userId, double newScore)
        {
            // First 100 Points Badge (Little Explorer)
            if (newScore >= 100)
            {
                await EnsureBadgeExists(
                    userId,
                    "Little Explorer 🌱",
                    "Congratulations! You have started your trust journey with 100 points.",
                    "icon_explorer.png"
                );
            }

            // 500 Points Badge (Grawiaa Hero)
            if (newScore >= 500)
            {
                await EnsureBadgeExists(
                    userId,
                    "Grawiaa Hero ⭐",
                    "You are now one of the highly committed heroes!",
                    "icon_hero.png"
                );
            }
        }

        private async Task EnsureBadgeExists(int userId, string name, string description, string icon)
        {
            var hasBadge = await _context.Badges
                .AnyAsync(b => b.ChildId == userId && b.Name == name);

            if (!hasBadge)
            {
                _context.Badges.Add(new Badge
                {
                    ChildId = userId,
                    Name = name,
                    Description = description,
                    IconUrl = icon,
                    UnlockedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}
