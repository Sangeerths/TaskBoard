using TaskBoard.EndpointService;
using TaskBoard.Models;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.LogicService
{
    public interface IProjectsLogic
    {
         Task<List<ProjectResponse>> GetProjectsAsync(Guid userId);
        Task<ProjectResponse> GetProjectAsync(Guid projectId, Guid userId);

        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId);

         Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, Guid userId);

         Task<ProjectResponse> DeleteProjectAsync(Guid projectId, Guid userId);
        Task<List<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId, Guid userId);
        Task<ProjectMemberResponse> AddProjectMemberAsync(Guid projectId, AddProjectMemberRequest request, Guid userId);
        Task<ProjectMemberResponse> UpdateProjectMemberAsync(Guid projectId, Guid targetUserId,UpdateProjectMemberRequest request, Guid userId);
        Task<ResponseBase> RemoveProjectMemberAsync(Guid projectId, Guid targetUserId, Guid userId);
    }
    public class ProjectsLogic : IProjectsLogic
    {
        private readonly IProjectsEndpointService _projectsEndpointService;

        public ProjectsLogic(IProjectsEndpointService projectsEndpointService)
        {
            _projectsEndpointService = projectsEndpointService;
        }

        public async Task<List<ProjectResponse>> GetProjectsAsync(Guid userId)
        {
            var projects = await _projectsEndpointService.GetProjectsAsync(userId);

            return projects.Select(p => new ProjectResponse
            {
                StatusCode = 200,
                Message = "Projects retrieved successfully",
                ProjectId = p.ProjectId,
                Name = p.Name,
                Description = p.Description,
                OwnerId = p.OwnerId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<ProjectResponse> GetProjectAsync(
         Guid projectId, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                return new ProjectResponse
                {
                    StatusCode = 404,
                    Message = "Project not found"
                };

            if (!await _projectsEndpointService.IsMemberAsync(projectId, userId))
                return new ProjectResponse
                {
                    StatusCode = 404,
                    Message = "Project not found"
                };

            return new ProjectResponse
            {
                StatusCode = 200,
                Message = "Project retrieved successfully",
                ProjectId = project.ProjectId,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return new ProjectResponse
                {
                    StatusCode = 400,
                    Message = "Project name is required"
                };

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _projectsEndpointService.CreateProjectAsync(project, userId);

            return new ProjectResponse
            {
                StatusCode = 201,
                Message = "Project created successfully",
                ProjectId = project.ProjectId,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        public async Task<ProjectResponse> UpdateProjectAsync(
    Guid projectId,
    UpdateProjectRequest request,
    Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
            {
                return new ProjectResponse
                {
                    StatusCode = 404,
                    Message = "Project not found"
                };
            }

            if (project.OwnerId != userId)
            {
                return new ProjectResponse
                {
                    StatusCode = 403,
                    Message = "Only the project owner can update this project"
                };
            }

            if (request.Name != null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new ProjectResponse
                    {
                        StatusCode = 400,
                        Message = "Project name cannot be empty"
                    };
                }

                project.Name = request.Name.Trim();
            }

            if (request.Description != null)
            {
                project.Description = request.Description.Trim();
            }

            project.UpdatedAt = DateTime.UtcNow;

            await _projectsEndpointService.SaveChangesAsync();

            return new ProjectResponse
            {
                StatusCode = 200,
                Message = "Project updated successfully",
                ProjectId = project.ProjectId,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        public async Task<ProjectResponse> DeleteProjectAsync(Guid projectId, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
               return new ProjectResponse
               {
                   StatusCode = 404,
                   Message = "Project not found"
               };

            if (project.OwnerId != userId)
                return new ProjectResponse
                {
                    StatusCode = 403,
                    Message = "Only the project owner can delete this project"
                };

            await _projectsEndpointService.DeleteProjectAsync(project);

            return new ProjectResponse
            {
                StatusCode = 200,
                Message = "Project deleted successfully"
            };
        }


        public async Task<List<ProjectMemberResponse>> GetProjectMembersAsync(
            Guid projectId,
            Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            var isMember = await _projectsEndpointService.IsMemberAsync(projectId, userId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException(
                    "You are not a member of this project");
            }

            var members = await _projectsEndpointService.GetProjectMembersAsync(projectId);

            return members.Select(pm => new ProjectMemberResponse
            {
                StatusCode = 200,
                Message = "Project members retrieved successfully",
                ProjectId = pm.ProjectId,
                UserId = pm.UserId,
                Email = pm.User.Email,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt
            }).ToList();
        }


      

        public async Task<ProjectMemberResponse> AddProjectMemberAsync(
            Guid projectId, AddProjectMemberRequest request, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (!await CanManageMembersAsync(projectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can manage members");

            var email = request.Email.Trim();
            var user = await _projectsEndpointService.GetUserByEmailAsync(email);

            if (user == null)
                throw new KeyNotFoundException("No user exists with that email");

            if (await _projectsEndpointService.IsMemberAsync(projectId, user.UserId))
                throw new InvalidOperationException(
                    "This user is already a project member");

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.UserId,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow
            };

            await _projectsEndpointService.AddProjectMemberAsync(member);

            return new ProjectMemberResponse
            {
                StatusCode = 201,
                Message = "Project member added successfully",
                ProjectId = projectId,
                UserId = user.UserId,
                Email = user.Email,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            };
        }

        public async Task<ProjectMemberResponse> UpdateProjectMemberAsync(
            Guid projectId, Guid targetUserId,
            UpdateProjectMemberRequest request, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (!await CanManageMembersAsync(projectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can manage members");

            var target = await _projectsEndpointService.GetProjectMemberAsync(
                projectId, targetUserId);

            if (target == null)
                throw new KeyNotFoundException(
                    "The specified user is not a member of this project");

            if (target.Role == "Owner")
                throw new InvalidOperationException(
                    "The project owner's role cannot be changed here");

            target.Role = request.Role;
            await _projectsEndpointService.SaveChangesAsync();

            return new ProjectMemberResponse
            {
                StatusCode = 200,
                Message = "Project member role updated successfully",
                ProjectId = projectId,
                UserId = target.UserId,
                Email = target.User.Email,
                Role = target.Role,
                JoinedAt = target.JoinedAt
            };
        }

        public async Task<ResponseBase> RemoveProjectMemberAsync(
            Guid projectId, Guid targetUserId, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (!await CanManageMembersAsync(projectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can manage members");

            var target = await _projectsEndpointService.GetProjectMemberAsync(
                projectId, targetUserId);

            if (target == null)
                throw new KeyNotFoundException(
                    "The specified user is not a member of this project");

            if (target.Role == "Owner" || project.OwnerId == targetUserId)
                throw new InvalidOperationException(
                    "The project owner cannot be removed");

            await _projectsEndpointService.RemoveProjectMemberAsync(target);

            return new ResponseBase
            {
                StatusCode = 200,
                Message = "Project member removed successfully"
            };
        }

        private async Task<bool> CanManageMembersAsync(
            Guid projectId, Guid userId)
        {
            var member = await _projectsEndpointService.GetProjectMemberAsync(
                projectId, userId);

            return member != null &&
                   (member.Role == "Owner" || member.Role == "Admin");
        }

    }
}
