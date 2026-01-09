using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using TaskManager.Models;
using TaskManager.Data;
namespace TaskManager.API
{
    [Route("tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTO to define exactly what the client is allowed to send
        public class CreateTaskRequest
        {
            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
            public string Title { get; set; } = string.Empty;
        }

        // DTO for updating tasks - only allow Title and IsDone to prevent UserId manipulation
        public class UpdateTaskRequest
        {
            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
            public string Title { get; set; } = string.Empty;

            public bool IsDone { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var tasks = await _context.Tasks.ToListAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve tasks", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var task = new TaskItem
                {
                    Title = request.Title,
                    IsDone = false,
                    UserId = 1 // Hardcoded User ID since Auth is out of scope
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create task", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null) return NotFound(new { error = $"Task with ID {id} not found" });

                // Only update allowed fields - prevents UserId manipulation
                task.Title = request.Title;
                task.IsDone = request.IsDone;
                await _context.SaveChangesAsync();

                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update task", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null) return NotFound(new { error = $"Task with ID {id} not found" });

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete task", details = ex.Message });
            }
        }
    }
}
