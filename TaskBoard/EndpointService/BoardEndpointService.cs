using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.EndpointService
{
    public interface IBoardEndpointService
    {
        Task<BoardColumn?> GetColumnAsync(Guid columnId);
        Task<int> GetColumnCountAsync(Guid projectId);
        Task AddColumnAsync(BoardColumn column);
        Task<List<BoardColumn>> GetColumnsForReorderAsync(Guid projectId);
        Task<bool> ProjectHasColumnAsync(Guid projectId, Guid columnId);
        Task DeleteColumnAsync(BoardColumn column);
        Task<List<BoardColumn>> GetBoardColumnsAsync(Guid projectId);
    }
    public class BoardEndpointService : IBoardEndpointService   
    {
        private readonly AppDbContext _dbContext;
        public BoardEndpointService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<BoardColumn?> GetColumnAsync(Guid columnId)
        {
            return await _dbContext.BoardColumns
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.ColumnId == columnId);
        }

        public async Task<int> GetColumnCountAsync(Guid projectId)
        {
            return await _dbContext.BoardColumns
                .CountAsync(c => c.ProjectId == projectId);
        }

        public async Task AddColumnAsync(BoardColumn column)
        {
            _dbContext.BoardColumns.Add(column);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<BoardColumn>> GetColumnsForReorderAsync(
            Guid projectId)
        {
            return await _dbContext.BoardColumns
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<bool> ProjectHasColumnAsync(
            Guid projectId, Guid columnId)
        {
            return await _dbContext.BoardColumns.AnyAsync(c =>
                c.ProjectId == projectId && c.ColumnId == columnId);
        }

        public async Task DeleteColumnAsync(BoardColumn column)
        {
            _dbContext.BoardColumns.Remove(column);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<List<BoardColumn>> GetBoardColumnsAsync(
            Guid projectId)
        {
            return await _dbContext.BoardColumns
                .Where(c => c.ProjectId == projectId)
                .Include(c => c.Tasks)
                .AsNoTracking()
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

    }
}
