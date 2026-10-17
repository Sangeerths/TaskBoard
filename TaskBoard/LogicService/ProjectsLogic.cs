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
    }
}
