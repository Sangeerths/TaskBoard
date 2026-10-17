using Microsoft.EntityFrameworkCore;
using TaskBoard.Data;
using TaskBoard.Models;

namespace TaskBoard.EndpointService
{
    public interface IProjectsEndpointService
    {
        Task<List<Project>> GetProjectsAsync(Guid userId);
        Task<Project?> GetProjectAsync(Guid projectId);
        Task<bool> IsMemberAsync(Guid projectId, Guid userId);
        Task<Project> CreateProjectAsync(Project project, Guid ownerId);
        Task SaveChangesAsync();
        Task DeleteProjectAsync(Project project);
    }
    public class ProjectsEndpointService : IProjectsEndpointService
    {
        private readonly AppDbContext _dbContext;
        
        public ProjectsEndpointService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Project>> GetProjectsAsync(Guid userId)
        {
            return await _dbContext.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.Project)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Project?> GetProjectAsync(Guid projectId)
        {
            return await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }

        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _dbContext.ProjectMembers.AnyAsync(pm =>
                pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task<Project> CreateProjectAsync(
            Project project, Guid ownerId)
        {
            project.OwnerId = ownerId;

            // The creator is automatically added as an owner member.
            project.Members.Add(new ProjectMember
            {
                ProjectId = project.ProjectId,
                UserId = ownerId,
                Role = "Owner"
            });

            _dbContext.Projects.Add(project);

            // Both records are saved together.
            await _dbContext.SaveChangesAsync();

            return project;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(Project project)
        {
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
        }
    }
}
