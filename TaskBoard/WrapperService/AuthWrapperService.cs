
using Microsoft.AspNetCore.Identity.Data;
using TaskBoard.Logic;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface IAuthWrapperService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequestDTO request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<MeResponse> GetMeAsync(Guid userId);
    }
    public class AuthWrapperService : IAuthWrapperService
    {
        private readonly IAuthLogic _authLogic;
        private readonly ILogger<AuthWrapperService> _logger;
        public AuthWrapperService(IAuthLogic authLogic, ILogger<AuthWrapperService> logger)
        {
            _authLogic = authLogic;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequestDTO request)
        {
            try
            {
                return await _authLogic.RegisterAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");

                return new RegisterResponse
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred"
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                return await _authLogic.LoginAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");

                return new LoginResponse
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred"
                };
            }
        }

        public async Task<MeResponse> GetMeAsync(Guid userId)
        {
            try
            {
                return await _authLogic.GetMeAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retrieving profile failed");

                return new MeResponse
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred"
                };
            }
        }
    }
}


