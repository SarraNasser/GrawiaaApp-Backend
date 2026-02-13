using GrawiaaApp.API.Data;
using GrawiaaApp.API.DTOs;
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrawiaaApp.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            return Ok(new
            {
                totalParents = await _context.Users.CountAsync(u => u.Role == "Parent"),
                totalChildren = await _context.Users.CountAsync(u => u.Role == "Child"),
                totalLogs = await _context.DailyLogEntries.CountAsync(),
                avgTrustScore = await _context.Users.Where(u => u.Role == "Child").AverageAsync(u => (double?)u.TrustScore) ?? 0
            });
        }

        [HttpPost("hobbies")]
        public async Task<IActionResult> AddHobby([FromBody] AddHobbyDto dto)
        {
            if (await _context.Hobbies.AnyAsync(h => h.Name == dto.Name))
                return BadRequest("Hobby exists");

            _context.Hobbies.Add(new Hobby
            {
                Name = dto.Name,
                Icon = dto.Icon,
                Category = dto.Category
            });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hobby added!" });
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var users = await _context.Users
                .Where(u => u.FullName.Contains(query) || u.UserName.Contains(query))
                .Select(u => new { u.Id, u.FullName, u.Role, u.TrustScore })
                .Take(20)
                .ToListAsync();
            return Ok(users);
        }
    }

    public class AddHobbyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}