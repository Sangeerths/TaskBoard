using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;
using TaskBoard.WrapperService;

namespace TaskBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthWrapperService _authWrapper;

        public AuthController(IAuthWrapperService authWrapper)
        {
            _authWrapper = authWrapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDTO request)
        {
            var response = await _authWrapper.RegisterAsync(request);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var response = await _authWrapper.LoginAsync(request);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new
                {
                    StatusCode = 401,
                    Message = "Invalid user identity"
                });
            }

            var response = await _authWrapper.GetMeAsync(userId);

            return StatusCode(response.StatusCode, response);
        }
    }
}

