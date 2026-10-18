using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.EndpointService
{
    public interface ITasksEndpointService
    {
        Task<List<TaskItem>> GetTasksAsync(
            Guid projectId,
            Guid? assigneeId,
            string? priority,
            DateTime? dueDateFrom,
            DateTime? dueDateTo,
            string? searchText,
            int pageNumber,
            int pageSize);
        Task<int> GetTasksCountAsync(
            Guid projectId,
            Guid? assigneeId,
            string? priority,
            DateTime? dueDateFrom,
            DateTime? dueDateTo,
            string? searchText);
        Task<TaskItem?> GetTaskAsync(Guid taskId);
        Task<BoardColumn?> GetColumnAsync(Guid columnId);
        Task<bool> UserExistsAsync(Guid userId);
        Task AddTaskAsync(TaskItem task);
        void DeleteTask(TaskItem task);
        Task<List<TaskItem>> GetColumnTasksAsync(Guid columnId);
        Task SaveChangesAsync();
    }
    public class TasksEndpointService : ITasksEndpointService
    {
            private readonly AppDbContext _dbContext;

            public TasksEndpointService(AppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            // Get a paginated list of tasks with optional filters
            public async Task<List<TaskItem>> GetTasksAsync(
                Guid projectId,
                Guid? assigneeId,
                string? priority,
                DateTime? dueDateFrom,
                DateTime? dueDateTo,
                string? searchText,
                int pageNumber,
                int pageSize)
            {
                var query = _dbContext.TaskItems
                    .AsNoTracking()
                    .Where(t => t.Column.ProjectId == projectId);

                if (assigneeId.HasValue)
                {
                    query = query.Where(t => t.AssigneeId == assigneeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(priority))
                {
                    query = query.Where(t => t.Priority == priority);
                }

                if (dueDateFrom.HasValue)
                {
                    query = query.Where(t =>
                        t.DueDate.HasValue &&
                        t.DueDate.Value >= dueDateFrom.Value);
                }

                if (dueDateTo.HasValue)
                {
                    query = query.Where(t =>
                        t.DueDate.HasValue &&
                        t.DueDate.Value <= dueDateTo.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var search = searchText.Trim();

                    query = query.Where(t =>
                        t.Title.Contains(search) ||
                        (t.Description != null &&
                         t.Description.Contains(search)));
                }

                return await query
                    .OrderBy(t => t.Column.Position)
                    .ThenBy(t => t.Position)
                    .ThenBy(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Count matching tasks before pagination
            public async Task<int> GetTasksCountAsync(
                Guid projectId,
                Guid? assigneeId,
                string? priority,
                DateTime? dueDateFrom,
                DateTime? dueDateTo,
                string? searchText)
            {
                var query = _dbContext.TaskItems
                    .Where(t => t.Column.ProjectId == projectId);

                if (assigneeId.HasValue)
                    query = query.Where(t => t.AssigneeId == assigneeId.Value);

                if (!string.IsNullOrWhiteSpace(priority))
                    query = query.Where(t => t.Priority == priority);

                if (dueDateFrom.HasValue)
                    query = query.Where(t =>
                        t.DueDate.HasValue &&
                        t.DueDate.Value >= dueDateFrom.Value);

                if (dueDateTo.HasValue)
                    query = query.Where(t =>
                        t.DueDate.HasValue &&
                        t.DueDate.Value <= dueDateTo.Value);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var search = searchText.Trim();

                    query = query.Where(t =>
                        t.Title.Contains(search) ||
                        (t.Description != null &&
                         t.Description.Contains(search)));
                }

                return await query.CountAsync();
            }

            // Get one task
            public async Task<TaskItem?> GetTaskAsync(Guid taskId)
            {
                return await _dbContext.TaskItems
                    .Include(t => t.Column)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);
            }

            // Get a column
            public async Task<BoardColumn?> GetColumnAsync(Guid columnId)
            {
                return await _dbContext.BoardColumns
                    .Include(c => c.Project)
                    .FirstOrDefaultAsync(c => c.ColumnId == columnId);
            }

            // Check whether a user exists
            public async Task<bool> UserExistsAsync(Guid userId)
            {
                return await _dbContext.Users
                    .AnyAsync(u => u.UserId == userId && u.IsActive);
            }

            // Add a task
            public async Task AddTaskAsync(TaskItem task)
            {
                await _dbContext.TaskItems.AddAsync(task);
            }

            // Delete a task
            public void DeleteTask(TaskItem task)
            {
                _dbContext.TaskItems.Remove(task);
            }

            // Get tasks in a column for ordering
            public async Task<List<TaskItem>> GetColumnTasksAsync(Guid columnId)
            {
                return await _dbContext.TaskItems
                    .Where(t => t.ColumnId == columnId)
                    .OrderBy(t => t.Position)
                    .ToListAsync();
            }

            // Save changes
            public async Task SaveChangesAsync()
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }

