using TaskBoard.EndpointService;
using TaskBoard.Models;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.LogicService
{
    public interface ICommentsLogic
    {
        Task<CommentsListResponse> GetCommentsAsync(Guid taskId, Guid userId);
        Task<CommentResponse> CreateCommentAsync(
            Guid taskId, Guid userId, CreateCommentRequest request);
        Task<CommentResponse> UpdateCommentAsync(
            Guid commentId, Guid userId, UpdateCommentRequest request);
        Task<ResponseBase> DeleteCommentAsync(Guid commentId, Guid userId);
    }
    public class CommentsLogic : ICommentsLogic
    {
        private readonly ICommentsEndpointService _comments;
        private readonly IProjectsEndpointService _projects;

        public CommentsLogic(ICommentsEndpointService comments, IProjectsEndpointService projects)
        {
            _comments = comments;
            _projects = projects;
        }
        private async Task<bool> IsProjectMemberAsync(
                Guid projectId, Guid userId)
        {
            return await _projects.IsMemberAsync(projectId, userId);
        }

        private async Task<bool> CanManageCommentsAsync(
            Guid projectId, Guid userId)
        {
            var member = await _projects.GetProjectMemberAsync(
                projectId, userId);

            return member != null &&
                   (member.Role == "Owner" || member.Role == "Admin");
        }

        private static CommentItemResponse MapComment(Comment comment)
        {
            return new CommentItemResponse
            {
                CommentId = comment.CommentId,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        public async Task<CommentsListResponse> GetCommentsAsync(
            Guid taskId, Guid userId)
        {
            var projectId = await _comments.GetTaskProjectIdAsync(taskId);

            if (projectId == null)
            {
                return new CommentsListResponse
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await IsProjectMemberAsync(projectId.Value, userId))
            {
                return new CommentsListResponse
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project."
                };
            }

            var comments = await _comments.GetCommentsAsync(taskId);

            return new CommentsListResponse
            {
                StatusCode = 200,
                Message = "Comments retrieved successfully.",
                Comments = comments.Select(MapComment).ToList()
            };
        }

        public async Task<CommentResponse> CreateCommentAsync(
            Guid taskId,
            Guid userId,
            CreateCommentRequest request)
        {
            var projectId = await _comments.GetTaskProjectIdAsync(taskId);

            if (projectId == null)
            {
                return new CommentResponse
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await IsProjectMemberAsync(projectId.Value, userId))
            {
                return new CommentResponse
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new CommentResponse
                {
                    StatusCode = 400,
                    Message = "Comment content cannot be empty."
                };
            }

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _comments.AddCommentAsync(comment);
            await _comments.SaveChangesAsync();

            return new CommentResponse
            {
                StatusCode = 201,
                Message = "Comment added successfully.",
                Comment = MapComment(comment)
            };
        }

        public async Task<CommentResponse> UpdateCommentAsync(
            Guid commentId,
            Guid userId,
            UpdateCommentRequest request)
        {
            var comment = await _comments.GetCommentAsync(commentId);

            if (comment == null)
            {
                return new CommentResponse
                {
                    StatusCode = 404,
                    Message = "Comment not found."
                };
            }

            if (comment.UserId != userId)
            {
                return new CommentResponse
                {
                    StatusCode = 403,
                    Message = "You can only edit your own comments."
                };
            }

            var projectId =
                await _comments.GetTaskProjectIdAsync(comment.TaskId);

            if (projectId == null ||
                !await IsProjectMemberAsync(projectId.Value, userId))
            {
                return new CommentResponse
                {
                    StatusCode = 403,
                    Message = "You no longer have access to this project."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new CommentResponse
                {
                    StatusCode = 400,
                    Message = "Comment content cannot be empty."
                };
            }

            comment.Content = request.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await _comments.SaveChangesAsync();

            return new CommentResponse
            {
                StatusCode = 200,
                Message = "Comment updated successfully.",
                Comment = MapComment(comment)
            };
        }

        public async Task<ResponseBase> DeleteCommentAsync(
            Guid commentId, Guid userId)
        {
            var comment = await _comments.GetCommentAsync(commentId);

            if (comment == null)
            {
                return new ResponseBase
                {
                    StatusCode = 404,
                    Message = "Comment not found."
                };
            }

            var projectId =
                await _comments.GetTaskProjectIdAsync(comment.TaskId);

            if (projectId == null)
            {
                return new ResponseBase
                {
                    StatusCode = 404,
                    Message = "Task not found."
                };
            }

            if (!await IsProjectMemberAsync(projectId.Value, userId))
            {
                return new ResponseBase
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project."
                };
            }

            var isAuthor = comment.UserId == userId;
            var canManage = await CanManageCommentsAsync(
                projectId.Value, userId);

            if (!isAuthor && !canManage)
            {
                return new ResponseBase
                {
                    StatusCode = 403,
                    Message = "You can only delete your own comments."
                };
            }

            _comments.DeleteComment(comment);
            await _comments.SaveChangesAsync();

            return new ResponseBase
            {
                StatusCode = 200,
                Message = "Comment deleted successfully."
            };
        }
    }
}
