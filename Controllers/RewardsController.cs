
using GrawiaaApp.API.Data;
using GrawiaaApp.API.DTOs;
using GrawiaaApp.API.Interfaces;
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
    public class RewardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGamingService _gamingService;

        public RewardsController(ApplicationDbContext context, IGamingService gamingService)
        {
            _context = context;
            _gamingService = gamingService;
        }

        /// <summary>
        /// Add a new reward for a child (Parent only)
        /// </summary>
        [Authorize(Roles = "Parent")]
        [HttpPost("add-reward")]
        public async Task<IActionResult> AddReward(AddRewardDto dto)
        {
            var reward = new Reward
            {
                Title = dto.Title,
                Cost = dto.Cost,
                Description = dto.Description ?? string.Empty,
                ChildId = dto.ChildId,
                IsRedeemed = false
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reward added successfully." });
        }

        /// <summary>
        /// Get all available (not redeemed) rewards for the child
        /// </summary>
        [Authorize(Roles = "Child")]
        [HttpGet("my-available-rewards")]
        public async Task<IActionResult> GetMyRewards()
        {
            var childId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var rewards = await _context.Rewards
                .Where(r => r.ChildId == childId && !r.IsRedeemed)
                .ToListAsync();

            return Ok(rewards);
        }

        /// <summary>
        /// Claim a reward using child's points
        /// </summary>
        [Authorize(Roles = "Child")]
        [HttpPost("claim-reward")]
        public async Task<IActionResult> ClaimReward(ClaimRewardDto dto)
        {
            var childId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.ChildId == childId);

            if (reward == null)
                return NotFound("Reward not found.");

            if (reward.IsRedeemed)
                return BadRequest("This reward has already been claimed.");

            var child = await _context.Users.FindAsync(childId);
            if (child == null)
                return NotFound("Child not found.");

            if (child.TrustScore < reward.Cost)
                return BadRequest($"Your points ({child.TrustScore}) are insufficient for this reward ({reward.Cost}).");

            reward.IsRedeemed = true;

            // Deduct points via GamingService
            var newScore = await _gamingService.UpdateScore(childId, -reward.Cost);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Reward claimed successfully!", remainingScore = newScore });
        }
    }
}
