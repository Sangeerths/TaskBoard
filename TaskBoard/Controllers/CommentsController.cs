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
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsWrapperService _wrapper;

        public CommentsController(ICommentsWrapperService wrapper)
        {
            _wrapper = wrapper;
        }

        private bool TryGetUserId(out Guid userId)
        {
            return Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        // GET /api/tasks/{id}/comments
        [HttpGet("tasks/{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _wrapper.GetCommentsAsync(id, userId);

            return StatusCode(response.StatusCode, response);
        }

        // POST /api/tasks/{id}/comments
        [HttpPost("tasks/{id:guid}/comments")]
        public async Task<IActionResult> CreateComment(
            Guid id,
            [FromBody] CreateCommentRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _wrapper.CreateCommentAsync(
                id, userId, request);

            return StatusCode(response.StatusCode, response);
        }

        // PUT /api/comments/{id}
        [HttpPut("comments/{id:guid}")]
        public async Task<IActionResult> UpdateComment(
            Guid id,
            [FromBody] UpdateCommentRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _wrapper.UpdateCommentAsync(
                id, userId, request);

            return StatusCode(response.StatusCode, response);
        }

        // DELETE /api/comments/{id}
        [HttpDelete("comments/{id:guid}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _wrapper.DeleteCommentAsync(id, userId);

            return StatusCode(response.StatusCode, response);
        }
    }
}
