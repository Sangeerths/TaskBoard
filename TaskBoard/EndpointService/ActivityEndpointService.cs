using TaskBoard.Data;
using TaskBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.EndpointService
{
    public interface IActivityEndpointService
    {
        Task<List<ActivityLog>> GetTaskActivityAsync(Guid taskId, int pageNumber, int pageSize);
        Task<int> GetTaskActivityCountAsync(Guid taskId);
        Task<List<ActivityLog>> GetProjectActivityAsync(Guid projectId, int pageNumber, int pageSize);
        Task<int> GetProjectActivityCountAsync(Guid projectId);
        Task AddActivityAsync(ActivityLog activity);
        Task SaveChangesAsync();
        Task<Guid?> GetTaskProjectIdAsync(Guid taskId);
    }
    public class ActivityEndpointService : IActivityEndpointService
    {
        private readonly AppDbContext _dbContext;

        public ActivityEndpointService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ActivityLog>> GetTaskActivityAsync(
            Guid taskId, int pageNumber, int pageSize)
        {
            return await _dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => a.TaskId == taskId)
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.ActivityId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTaskActivityCountAsync(Guid taskId)
        {
            return await _dbContext.ActivityLogs
                .CountAsync(a => a.TaskId == taskId);
        }

        public async Task<List<ActivityLog>> GetProjectActivityAsync(
            Guid projectId, int pageNumber, int pageSize)
        {
            return await _dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => a.ProjectId == projectId)
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.ActivityId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetProjectActivityCountAsync(Guid projectId)
        {
            return await _dbContext.ActivityLogs
                .CountAsync(a => a.ProjectId == projectId);
        }

        public async Task AddActivityAsync(ActivityLog activity)
        {
            await _dbContext.ActivityLogs.AddAsync(activity);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Guid?> GetTaskProjectIdAsync(Guid taskId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.TaskId == taskId)
                .Select(t => (Guid?)t.Column.ProjectId)
                .FirstOrDefaultAsync();
        }
    }
}