using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;
using TaskBoard.WrapperService;

namespace TaskBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardWrapperService _boardWrapperService;
        public BoardController(IBoardWrapperService boardWrapperService)
        {
            _boardWrapperService = boardWrapperService;
        }
        private bool TryGetUserId(out Guid userId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out userId);
        }

        [HttpGet("{id:guid}/board")]
        public async Task<IActionResult> GetBoard(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _boardWrapperService.GetBoardAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{id:guid}/columns")]
        public async Task<IActionResult> CreateColumn(
            Guid id, [FromBody] CreateColumnRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _boardWrapperService.CreateColumnAsync(
                    id, request, userId);

                return CreatedAtAction(
                    nameof(GetBoard), new { id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseBase
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("/api/columns/{id:guid}")]
        public async Task<IActionResult> RenameColumn(
            Guid id, [FromBody] RenameColumnRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _boardWrapperService.RenameColumnAsync(
                    id, request, userId);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseBase
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id:guid}/columns/reorder")]
        public async Task<IActionResult> ReorderColumns(
            Guid id, [FromBody] ReorderColumnsRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _boardWrapperService.ReorderColumnsAsync(
                    id, request, userId);

                return StatusCode(response.StatusCode, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseBase
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("/api/columns/{id:guid}")]
        public async Task<IActionResult> DeleteColumn(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _boardWrapperService.DeleteColumnAsync(id, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseBase
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }

    }
}
