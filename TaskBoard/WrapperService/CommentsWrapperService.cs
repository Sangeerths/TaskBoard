using TaskBoard.LogicService;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface ICommentsWrapperService
    {
        Task<CommentsListResponse> GetCommentsAsync(
            Guid taskId, Guid userId);
        Task<CommentResponse> CreateCommentAsync(
            Guid taskId, Guid userId, CreateCommentRequest request);
        Task<CommentResponse> UpdateCommentAsync(
            Guid commentId, Guid userId, UpdateCommentRequest request);
        Task<ResponseBase> DeleteCommentAsync(
            Guid commentId, Guid userId);
    }
    public class CommentsWrapperService : ICommentsWrapperService
    {
        private readonly ICommentsLogic _commentsLogic;
        public CommentsWrapperService(ICommentsLogic commentsLogic)
        {
            _commentsLogic = commentsLogic;
        }

        public Task<CommentsListResponse> GetCommentsAsync(
          Guid taskId, Guid userId)
        {
            return _commentsLogic.GetCommentsAsync(taskId, userId);
        }

        public Task<CommentResponse> CreateCommentAsync(
            Guid taskId,
            Guid userId,
            CreateCommentRequest request)
        {
            return _commentsLogic.CreateCommentAsync(taskId, userId, request);
        }

        public Task<CommentResponse> UpdateCommentAsync(
            Guid commentId,
            Guid userId,
            UpdateCommentRequest request)
        {
            return _commentsLogic.UpdateCommentAsync(commentId, userId, request);
        }

        public Task<ResponseBase> DeleteCommentAsync(
            Guid commentId, Guid userId)
        {
            return _commentsLogic.DeleteCommentAsync(commentId, userId);
        }
    }
}
