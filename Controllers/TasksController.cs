
using GrawiaaApp.API.Data;
using GrawiaaApp.API.DTOs;
using GrawiaaApp.API.Models;
using GrawiaaApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrawiaaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGamingService _gamingService;

        public TasksController(ApplicationDbContext context, IGamingService gamingService)
        {
            _context = context;
            _gamingService = gamingService;
        }

        [Authorize(Roles = "Parent")]
        [HttpPost("add-task")]
        public async Task<IActionResult> CreateTask(AddTaskDto dto)
        {
            var task = new ChildTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Points = dto.Points,
                ChildId = dto.ChildId,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.ChildTasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task added." });
        }

        [Authorize(Roles = "Child")]
        [HttpPost("complete-task/{taskId}")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var childId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var task = await _context.ChildTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ChildId == childId);

            if (task == null || task.IsCompleted)
                return BadRequest("Task not found or already done.");

            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;

            // ✅ التعديل هنا: استخدام GamingService
            var newScore = await _gamingService.UpdateScore(childId, task.Points);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Success!", newScore });
        }
    }
}
