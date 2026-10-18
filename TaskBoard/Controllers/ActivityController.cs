using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.WrapperService;

namespace TaskBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityWrapperService _activityWrapperService;
        public ActivityController(IActivityWrapperService activityWrapperService)
        {
            _activityWrapperService = activityWrapperService;
        }
        private bool TryGetUserId(out Guid userId)
        {
            return Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        // GET /api/tasks/{id}/activity
        [HttpGet("tasks/{id:guid}/activity")]
        public async Task<IActionResult> GetTaskActivity(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _activityWrapperService.GetTaskActivityAsync(
                id, userId, pageNumber, pageSize);

            return StatusCode(response.StatusCode, response);
        }

        // GET /api/projects/{id}/activity
        [HttpGet("projects/{id:guid}/activity")]
        public async Task<IActionResult> GetProjectActivity(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _activityWrapperService.GetProjectActivityAsync(
                id, userId, pageNumber, pageSize);

            return StatusCode(response.StatusCode, response);
        }
    }
}