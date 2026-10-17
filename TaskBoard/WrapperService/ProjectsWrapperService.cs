using TaskBoard.LogicService;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface IProjectsWrapperService
    {
        Task<List<ProjectResponse>> GetProjectsAsync(Guid userId);
        Task<ProjectResponse> GetProjectAsync(Guid projectId, Guid userId);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId);
        Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, Guid userId);
        Task<ProjectResponse> DeleteProjectAsync(Guid projectId, Guid userId);
    }
    public class ProjectsWrapperService : IProjectsWrapperService
    {
        private readonly IProjectsLogic _projectsLogic;
        public ProjectsWrapperService(IProjectsLogic projectsLogic)
        {
            _projectsLogic = projectsLogic;
        }

        public Task<List<ProjectResponse>> GetProjectsAsync(Guid userId)
        {
            return _projectsLogic.GetProjectsAsync(userId);
        }

        public Task<ProjectResponse> GetProjectAsync(
            Guid projectId, Guid userId)
        {
            return _projectsLogic.GetProjectAsync(projectId, userId);
        }

        public Task<ProjectResponse> CreateProjectAsync(
            CreateProjectRequest request, Guid userId)
        {
            return _projectsLogic.CreateProjectAsync(request, userId);
        }

        public Task<ProjectResponse> UpdateProjectAsync(
            Guid projectId,
            UpdateProjectRequest request,
            Guid userId)
        {
            return _projectsLogic.UpdateProjectAsync(projectId, request, userId);
        }

        public Task<ProjectResponse> DeleteProjectAsync(
            Guid projectId, Guid userId)
        {
            return _projectsLogic.DeleteProjectAsync(projectId, userId);
        }
    }
}
