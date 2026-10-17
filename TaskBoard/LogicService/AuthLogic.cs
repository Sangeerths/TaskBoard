using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskBoard.EndpointServices;
using TaskBoard.Models;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.Logic
{
    public interface IAuthLogic
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequestDTO request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<MeResponse> GetMeAsync(Guid userId);
    }
    public class AuthLogic : IAuthLogic
    {
        private readonly IAuthEndpointService _endpointService;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthLogic(IAuthEndpointService endpointService, IConfiguration configuration)
        {
            _endpointService = endpointService;
            _configuration = configuration;
        }

        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequestDTO request)
        {
                  
            var email = request.Email.Trim();

            var existingUser =
                await _endpointService.GetUserByEmailAsync(email);

            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    StatusCode = 409,
                    Message = "Email already exists",
                    Email = email
                };
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            user.UserProfile = new UserProfile
            {
                ProfileId = Guid.NewGuid(),
                UserId = user.UserId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName,
                UpdatedAt = DateTime.UtcNow
            };

            await _endpointService.CreateUserAsync(user);

            return new RegisterResponse
            {
                StatusCode = 201,
                Message = "User created successfully",
                Email = user.Email
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _endpointService.GetUserByEmailAsync(
                request.Email.Trim());

            if (user == null || !user.IsActive)
            {
                return new LoginResponse
                {
                    StatusCode = 401,
                    Message = "Invalid email or password"
                };
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return new LoginResponse
                {
                    StatusCode = 401,
                    Message = "Invalid email or password"
                };
            }

            var secret = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException(
                    "JWT secret is not configured.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var expiresAt = DateTime.UtcNow.AddHours(1);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new LoginResponse
            {
                StatusCode = 200,
                Message = "Login successful",
                Token = new JwtSecurityTokenHandler()
                    .WriteToken(token),
                ExpiresAt = expiresAt
            };
        }

        public async Task<MeResponse> GetMeAsync(Guid userId)
        {
            var user = await _endpointService.GetUserByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return new MeResponse
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            var profile = user.UserProfile;

            return new MeResponse
            {
                StatusCode = 200,
                Message = "Profile retrieved successfully",
                UserId = user.UserId,
                Email = user.Email,
                FirstName = profile?.FirstName ?? string.Empty,
                LastName = profile?.LastName,
                PhoneNumber = profile?.PhoneNumber,
                DateOfBirth = profile?.DateOfBirth,
                Gender = profile?.Gender,
                Address = profile?.Address,
                City = profile?.City,
                State = profile?.State,
                Country = profile?.Country,
                PostalCode = profile?.PostalCode
            };
        }
    }
}