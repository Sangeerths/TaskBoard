using TaskBoard.LogicService;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface IActivityWrapperService
    {
        Task<ActivityListResponse> GetTaskActivityAsync(
          Guid taskId, Guid userId, int pageNumber, int pageSize);
        Task<ActivityListResponse> GetProjectActivityAsync(
            Guid projectId, Guid userId, int pageNumber, int pageSize);

    }
    public class ActivityWrapperService : IActivityWrapperService
    {
        private readonly IActivityLogic _activityLogic;
        public ActivityWrapperService(IActivityLogic activityLogic)
        {
            _activityLogic = activityLogic;
        }
        public Task<ActivityListResponse> GetTaskActivityAsync(
          Guid taskId, Guid userId, int pageNumber, int pageSize)
        {
            return _activityLogic.GetTaskActivityAsync(
                taskId, userId, pageNumber, pageSize);
        }

        public Task<ActivityListResponse> GetProjectActivityAsync(
            Guid projectId, Guid userId, int pageNumber, int pageSize)
        {
            return _activityLogic.GetProjectActivityAsync(
                projectId, userId, pageNumber, pageSize);
        }
    }
}
