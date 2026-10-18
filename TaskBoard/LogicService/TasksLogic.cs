using TaskBoard.EndpointService;
using TaskBoard.Models;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.LogicService
{
    public interface ITasksLogic
    {
        Task<TasksListResponse> GetTasksAsync(
            Guid projectId,
            Guid userId,
            TaskFilterRequest request);
        Task<TaskResponse> CreateTaskAsync(
            Guid columnId,
            Guid userId,
            CreateTaskRequest request);
        Task<TaskResponse> GetTaskAsync(
            Guid taskId,
            Guid userId);

        Task<TaskResponse> UpdateTaskAsync(
            Guid taskId,
            Guid userId,
            UpdateTaskRequest request);
        Task<TaskResponse> MoveTaskAsync(
            Guid taskId,
            Guid userId,
            MoveTaskRequest request);
        Task<ResponseBase> DeleteTaskAsync(
            Guid taskId,
            Guid userId);

    }
    public class TasksLogic : ITasksLogic
    {
        private readonly ITasksEndpointService _tasksEndpointService;
        private readonly IProjectsEndpointService _projectsEndpointService;
        public TasksLogic(ITasksEndpointService tasksEndpointService, IProjectsEndpointService projectsEndpointService)
        {
            _tasksEndpointService = tasksEndpointService;
            _projectsEndpointService = projectsEndpointService;
        }

        private static bool IsValidPriority(string priority)
        {
            return new[] { "Low", "Medium", "High", "Critical" }
                .Contains(priority, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<bool> CanAccessProjectAsync(
            Guid projectId,
            Guid userId)
        {
            return await _projectsEndpointService.IsMemberAsync(
                projectId, userId);
        }

        private async Task<bool> CanManageTasksAsync(
            Guid projectId,
            Guid userId)
        {
            var member = await _projectsEndpointService
                .GetProjectMemberAsync(projectId, userId);

            return member != null &&
                   (member.Role == "Owner" || member.Role == "Admin");
        }

        private static TaskResponse MapTask(
            TaskItem task,
            int statusCode = 200,
            string message = "Success")
        {
            return new TaskResponse
            {
                StatusCode = statusCode,
                Message = message,
                TaskId = task.TaskId,
                ColumnId = task.ColumnId,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                AssigneeId = task.AssigneeId,
                Position = task.Position,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        // GET /projects/{id}/tasks
        public async Task<TasksListResponse> GetTasksAsync(
            Guid projectId,
            Guid userId,
            TaskFilterRequest request)
        {
            var project = await _projectsEndpointService
                .GetProjectAsync(projectId);

            if (project == null)
            {
                return new TasksListResponse
                {
                    StatusCode = 404,
                    Message = "Project not found."
                };
            }

            if (!await CanAccessProjectAsync(projectId, userId))
            {
                return new TasksListResponse
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project."
                };
            }

            if (request.DueDateFrom.HasValue &&
                request.DueDateTo.HasValue &&
                request.DueDateFrom > request.DueDateTo)
            {
                return new TasksListResponse
                {
                    StatusCode = 400,
                    Message = "DueDateFrom cannot be later than DueDateTo."
                };
            }

            var tasks = await _tasksEndpointService.GetTasksAsync(
                projectId,
                request.AssigneeId,
                request.Priority,
                request.DueDateFrom,
                request.DueDateTo,
                request.SearchText,
                request.PageNumber,
                request.PageSize);

            var totalRecords = await _tasksEndpointService.GetTasksCountAsync(
                projectId,
                request.AssigneeId,
                request.Priority,
                request.DueDateFrom,
                request.DueDateTo,
                request.SearchText);

            return new TasksListResponse
            {
                StatusCode = 200,
                Message = "Tasks retrieved successfully.",
                Tasks = tasks.Select(t => MapTask(t)).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)request.PageSize)
            };
        }

        // POST /columns/{columnId}/tasks
        public async Task<TaskResponse> CreateTaskAsync(
            Guid columnId,
            Guid userId,
            CreateTaskRequest request)
        {
            var column = await _tasksEndpointService.GetColumnAsync(columnId);

            if (column == null)
            {
                return new TaskResponse
                {
                    StatusCode = 404,
                    Message = "Column not found."
                };
            }

            if (!await CanManageTasksAsync(column.ProjectId, userId))
            {
                return new TaskResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to create tasks."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new TaskResponse
                {
                    StatusCode = 400,
                    Message = "Task title cannot be empty."
                };
            }

            if (!IsValidPriority(request.Priority))
            {
                return new TaskResponse
                {
                    StatusCode = 400,
                    Message = "Invalid priority."
                };
            }

            if (request.AssigneeId.HasValue)
            {
                if (!await _tasksEndpointService.UserExistsAsync(
                    request.AssigneeId.Value))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Assignee not found or inactive."
                    };
                }

                if (!await _projectsEndpointService.IsMemberAsync(
                    column.ProjectId, request.AssigneeId.Value))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Assignee must be a project member."
                    };
                }
            }

            var existingTasks =
                await _tasksEndpointService.GetColumnTasksAsync(columnId);

            var task = new TaskItem
            {
                ColumnId = columnId,
                Title = request.Title.Trim(),
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                AssigneeId = request.AssigneeId,
                Position = existingTasks.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tasksEndpointService.AddTaskAsync(task);
            await _tasksEndpointService.SaveChangesAsync();

            return MapTask(task, 201, "Task created successfully.");
        }

        // GET /tasks/{id}
        public async Task<TaskResponse> GetTaskAsync(
            Guid taskId,
            Guid userId)
        {
            var task = await _tasksEndpointService.GetTaskAsync(taskId);

            if (task == null)
            {
                return new TaskResponse
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await CanAccessProjectAsync(task.Column.ProjectId, userId))
            {
                return new TaskResponse
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project."
                };
            }

            return MapTask(task);
        }

        // PUT /tasks/{id}
        public async Task<TaskResponse> UpdateTaskAsync(
            Guid taskId,
            Guid userId,
            UpdateTaskRequest request)
        {
            var task = await _tasksEndpointService.GetTaskAsync(taskId);

            if (task == null)
            {
                return new TaskResponse
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await CanManageTasksAsync(task.Column.ProjectId, userId))
            {
                return new TaskResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to update tasks."
                };
            }

            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Task title cannot be empty."
                    };
                }

                task.Title = request.Title.Trim();
            }

            if (request.Description != null)
                task.Description = request.Description;

            if (request.Priority != null)
            {
                if (!IsValidPriority(request.Priority))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Invalid priority."
                    };
                }

                task.Priority = request.Priority;
            }

            if (request.AssigneeId.HasValue)
            {
                if (!await _tasksEndpointService.UserExistsAsync(
                    request.AssigneeId.Value))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Assignee not found or inactive."
                    };
                }

                if (!await _projectsEndpointService.IsMemberAsync(
                    task.Column.ProjectId, request.AssigneeId.Value))
                {
                    return new TaskResponse
                    {
                        StatusCode = 400,
                        Message = "Assignee must be a project member."
                    };
                }

                task.AssigneeId = request.AssigneeId.Value;
            }

            if (request.DueDate.HasValue)
                task.DueDate = request.DueDate.Value;

            task.UpdatedAt = DateTime.UtcNow;

            await _tasksEndpointService.SaveChangesAsync();

            return MapTask(task, 200, "Task updated successfully.");
        }

        // DELETE /tasks/{id}
        public async Task<ResponseBase> DeleteTaskAsync(
            Guid taskId,
            Guid userId)
        {
            var task = await _tasksEndpointService.GetTaskAsync(taskId);

            if (task == null)
            {
                return new ResponseBase
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await CanManageTasksAsync(task.Column.ProjectId, userId))
            {
                return new ResponseBase
                {
                    StatusCode = 403,
                    Message = "You do not have permission to delete tasks."
                };
            }

            _tasksEndpointService.DeleteTask(task);

            var remainingTasks =
                await _tasksEndpointService.GetColumnTasksAsync(task.ColumnId);

            for (var i = 0; i < remainingTasks.Count; i++)
                remainingTasks[i].Position = i;

            await _tasksEndpointService.SaveChangesAsync();

            return new ResponseBase
            {
                StatusCode = 200,
                Message = "Task deleted successfully."
            };
        }

        // PUT /tasks/{id}/move
        public async Task<TaskResponse> MoveTaskAsync(
            Guid taskId,
            Guid userId,
            MoveTaskRequest request)
        {
            var task = await _tasksEndpointService.GetTaskAsync(taskId);

            if (task == null)
            {
                return new TaskResponse
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            var targetColumn =
                await _tasksEndpointService.GetColumnAsync(request.ColumnId);

            if (targetColumn == null)
            {
                return new TaskResponse
                {
                    StatusCode = 404,
                    Message = "Destination column not found."
                };
            }

            if (targetColumn.ProjectId != task.Column.ProjectId)
            {
                return new TaskResponse
                {
                    StatusCode = 400,
                    Message = "Tasks cannot be moved between different projects."
                };
            }

            if (!await CanManageTasksAsync(task.Column.ProjectId, userId))
            {
                return new TaskResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to move tasks."
                };
            }

            var sourceColumnId = task.ColumnId;

            var sourceTasks =
                await _tasksEndpointService.GetColumnTasksAsync(sourceColumnId);

            var targetTasks = sourceColumnId == request.ColumnId
                ? sourceTasks
                : await _tasksEndpointService.GetColumnTasksAsync(request.ColumnId);

            sourceTasks.RemoveAll(t => t.TaskId == task.TaskId);

            if (sourceColumnId == request.ColumnId)
            {
                targetTasks = sourceTasks;
            }

            var newPosition = Math.Min(request.Position, targetTasks.Count);

            task.ColumnId = request.ColumnId;
            targetTasks.Insert(newPosition, task);

            for (var i = 0; i < targetTasks.Count; i++)
                targetTasks[i].Position = i;

            if (sourceColumnId != request.ColumnId)
            {
                for (var i = 0; i < sourceTasks.Count; i++)
                    sourceTasks[i].Position = i;
            }

            task.UpdatedAt = DateTime.UtcNow;

            await _tasksEndpointService.SaveChangesAsync();

            return MapTask(task, 200, "Task moved successfully.");
        }
    }
}