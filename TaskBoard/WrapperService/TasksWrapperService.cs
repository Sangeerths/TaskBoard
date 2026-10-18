using TaskBoard.LogicService;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface ITasksWrapperService
    {
        Task<TasksListResponse> GetTasksAsync(
            Guid projectId,
            Guid userId,
            TaskFilterRequest request);
        Task<TaskResponse> CreateTaskAsync(
            Guid columnId,
            Guid userId,
            CreateTaskRequest request);
        Task<TaskResponse> GetTaskAsync(Guid taskId, Guid userId);
        Task<TaskResponse> UpdateTaskAsync(
            Guid taskId,
            Guid userId,
            UpdateTaskRequest request);
        Task<ResponseBase> DeleteTaskAsync(Guid taskId, Guid userId);
        Task<TaskResponse> MoveTaskAsync(
            Guid taskId,
            Guid userId,
            MoveTaskRequest request);
    }
    public class TasksWrapperService : ITasksWrapperService
    {
        private readonly ITasksLogic _tasksLogic;
        public TasksWrapperService(ITasksLogic tasksLogic)
        {
            _tasksLogic = tasksLogic;
        }
        public Task<TasksListResponse> GetTasksAsync(
           Guid projectId,
           Guid userId,
           TaskFilterRequest request)
        {
            return _tasksLogic.GetTasksAsync(projectId, userId, request);
        }

        public Task<TaskResponse> CreateTaskAsync(
            Guid columnId,
            Guid userId,
            CreateTaskRequest request)
        {
            return _tasksLogic.CreateTaskAsync(columnId, userId, request);
        }

        public Task<TaskResponse> GetTaskAsync(Guid taskId, Guid userId)
        {
            return _tasksLogic.GetTaskAsync(taskId, userId);
        }

        public Task<TaskResponse> UpdateTaskAsync(
            Guid taskId,
            Guid userId,
            UpdateTaskRequest request)
        {
            return _tasksLogic.UpdateTaskAsync(taskId, userId, request);
        }

        public Task<ResponseBase> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            return _tasksLogic.DeleteTaskAsync(taskId, userId);
        }

        public Task<TaskResponse> MoveTaskAsync(
            Guid taskId,
            Guid userId,
            MoveTaskRequest request)
        {
            return _tasksLogic.MoveTaskAsync(taskId, userId, request);
        }
    }
}
