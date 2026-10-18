using System.Diagnostics;
using TaskBoard.EndpointService;
using TaskBoard.Models;
using TaskBoard.Models.Responses;

namespace TaskBoard.LogicService
{
    public interface IActivityLogic
    {
       Task<ActivityListResponse> GetTaskActivityAsync(
            Guid taskId, Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<ActivityListResponse> GetProjectActivityAsync(
            Guid projectId, Guid userId, int pageNumber = 1, int pageSize = 20);


    }
    public class ActivityLogic : IActivityLogic
    {
        private readonly IActivityEndpointService _activityEndpointService;
        private readonly IProjectsEndpointService _projectEndpointService;
        public ActivityLogic(IActivityEndpointService activityEndpointService, IProjectsEndpointService projectEndpointService)
        {
            _activityEndpointService = activityEndpointService;
            _projectEndpointService = projectEndpointService;
        }
        private static ActivityItemResponse MapActivity(ActivityLog item)
        {
            return new ActivityItemResponse
            {
                ActivityId = item.ActivityId,
                ProjectId = item.ProjectId,
                TaskId = item.TaskId,
                ActorId = item.ActorId,
                Action = item.Action,
                Description = item.Description,
                CreatedAt = item.CreatedAt
            };
        }

        private static ActivityListResponse Error(
            int statusCode, string message)
        {
            return new ActivityListResponse
            {
                StatusCode = statusCode,
                Message = message
            };
        }

        private static ActivityListResponse BuildResponse(
            List<ActivityLog> items,
            int totalRecords,
            int pageNumber,
            int pageSize)
        {
            return new ActivityListResponse
            {
                StatusCode = 200,
                Message = "Activity retrieved successfully.",
                Activities = items.Select(MapActivity).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)pageSize)
            };
        }

        public async Task<ActivityListResponse> GetTaskActivityAsync(
            Guid taskId,
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return Error(400, "Invalid pagination values.");

            var projectId = await _activityEndpointService.GetTaskProjectIdAsync(taskId);

            if (projectId == null)
                return Error(404, "Task not found.");

            if (!await _projectEndpointService.IsMemberAsync(projectId.Value, userId))
                return Error(403, "You are not a member of this project.");

            var items = await _activityEndpointService.GetTaskActivityAsync(
                taskId, pageNumber, pageSize);

            var count = await _activityEndpointService.GetTaskActivityCountAsync(taskId);

            return BuildResponse(items, count, pageNumber, pageSize);
        }

        public async Task<ActivityListResponse> GetProjectActivityAsync(
            Guid projectId,
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return Error(400, "Invalid pagination values.");

            var project = await _projectEndpointService.GetProjectAsync(projectId);

            if (project == null)
                return Error(404, "Project not found.");

            if (!await _projectEndpointService.IsMemberAsync(projectId, userId))
                return Error(403, "You are not a member of this project.");

            var items = await _activityEndpointService.GetProjectActivityAsync(
                projectId, pageNumber, pageSize);

            var count = await _activityEndpointService.GetProjectActivityCountAsync(projectId);

            return BuildResponse(items, count, pageNumber, pageSize);
        }
    }
}