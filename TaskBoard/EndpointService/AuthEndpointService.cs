using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.EndpointServices
{
    public interface IAuthEndpointService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid userId);
    }
    public class AuthEndpointService : IAuthEndpointService
    {
        private readonly AppDbContext _dbContext;

        public AuthEndpointService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            return _dbContext.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task CreateUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public Task<User?> GetUserByIdAsync(Guid userId)
        {
            return _dbContext.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}