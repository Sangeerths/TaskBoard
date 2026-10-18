using Microsoft.AspNetCore.Identity;
using TaskBoard.Data;
using TaskBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.EndpointService
{
    public interface ICommentsEndpointService
    {
        Task<bool> TaskExistsAsync(Guid taskId);
        Task<Guid?> GetTaskProjectIdAsync(Guid taskId);
        Task<List<Comment>> GetCommentsAsync(Guid taskId);
        Task<Comment?> GetCommentAsync(Guid commentId);
        Task AddCommentAsync(Comment comment);
        void DeleteComment(Comment comment);
        Task SaveChangesAsync();
    }
    public class CommentsEndpointService : ICommentsEndpointService
    {
        private readonly AppDbContext _dbContext;

        public CommentsEndpointService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> TaskExistsAsync(Guid taskId)
        {
            return await _dbContext.TaskItems
                .AnyAsync(t => t.TaskId == taskId);
        }

        public async Task<Guid?> GetTaskProjectIdAsync(Guid taskId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.TaskId == taskId)
                .Select(t => (Guid?)t.Column.ProjectId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Comment>> GetCommentsAsync(Guid taskId)
        {
            return await _dbContext.Comments
                .AsNoTracking()
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentAsync(Guid commentId)
        {
            return await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _dbContext.Comments.AddAsync(comment);
        }

        public void DeleteComment(Comment comment)
        {
            _dbContext.Comments.Remove(comment);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
