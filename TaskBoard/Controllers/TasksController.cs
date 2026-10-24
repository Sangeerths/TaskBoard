using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.Models.Requests;
using TaskBoard.WrapperService;

namespace TaskBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITasksWrapperService _tasksWrapperService;

        public TasksController(ITasksWrapperService tasksWrapperService)
        {
            _tasksWrapperService = tasksWrapperService;
        }
        private bool TryGetUserId(out Guid userId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out userId);
        }

        // GET /api/projects/{id}/tasks
        [HttpGet("projects/{id:guid}/tasks")]
        public async Task<IActionResult> GetTasks(
            Guid id,
            [FromQuery] TaskFilterRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.GetTasksAsync(
                id, userId, request);

            return StatusCode(response.StatusCode, response);
        }

        // POST /api/columns/{columnId}/tasks
        [HttpPost("columns/{columnId:guid}/tasks")]
        public async Task<IActionResult> CreateTask(
            Guid columnId,
            [FromBody] CreateTaskRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.CreateTaskAsync(
                columnId, userId, request);

            return StatusCode(response.StatusCode, response);
        }

        // GET /api/tasks/{id}
        [HttpGet("tasks/{id:guid}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.GetTaskAsync(
                id, userId);

            return StatusCode(response.StatusCode, response);
        }

        // PUT /api/tasks/{id}
        [HttpPut("tasks/{id:guid}")]
        public async Task<IActionResult> UpdateTask(
            Guid id,
            [FromBody] UpdateTaskRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.UpdateTaskAsync(
                id, userId, request);

            return StatusCode(response.StatusCode, response);
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("tasks/{id:guid}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.DeleteTaskAsync(
                id, userId);

            return StatusCode(response.StatusCode, response);
        }

        // PUT /api/tasks/{id}/move
        [HttpPut("tasks/{id:guid}/move")]
        public async Task<IActionResult> MoveTask(
            Guid id,
            [FromBody] MoveTaskRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _tasksWrapperService.MoveTaskAsync(
                id, userId, request);

            return StatusCode(response.StatusCode, response);
        }
    }
}
