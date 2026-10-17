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
        Task<List<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId, Guid userId);
        Task<ProjectMemberResponse> AddProjectMemberAsync(Guid projectId, AddProjectMemberRequest request, Guid userId);
        Task<ProjectMemberResponse> UpdateProjectMemberAsync(Guid projectId, Guid targetUserId, UpdateProjectMemberRequest request, Guid userId);
        Task<ResponseBase> RemoveProjectMemberAsync(Guid projectId, Guid targetUserId, Guid userId);
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


        public Task<List<ProjectMemberResponse>> GetProjectMembersAsync(
            Guid projectId,
            Guid userId)
        {
            return _projectsLogic.GetProjectMembersAsync(projectId, userId);
        }


       

        public Task<ProjectMemberResponse> AddProjectMemberAsync(
            Guid projectId, AddProjectMemberRequest request, Guid userId)
        {
            return _projectsLogic.AddProjectMemberAsync(projectId, request, userId);
        }

        public Task<ProjectMemberResponse> UpdateProjectMemberAsync(
            Guid projectId, Guid targetUserId,
            UpdateProjectMemberRequest request, Guid userId)
        {
            return _projectsLogic.UpdateProjectMemberAsync(
                projectId, targetUserId, request, userId);
        }

        public Task<ResponseBase> RemoveProjectMemberAsync(
            Guid projectId, Guid targetUserId, Guid userId)
        {
            return _projectsLogic.RemoveProjectMemberAsync(
                projectId, targetUserId, userId);
        }


    }
}
