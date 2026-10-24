using TaskBoard.EndpointService;
using TaskBoard.Models;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.LogicService
{
    public interface IBoardLogic
    {
         Task<BoardResponse> GetBoardAsync(Guid projectId, Guid userId);
        Task<BoardColumnResponse> CreateColumnAsync(
            Guid projectId, CreateColumnRequest request, Guid userId);
        Task<BoardColumnResponse> RenameColumnAsync(
            Guid columnId, RenameColumnRequest request, Guid userId);
        Task<ResponseBase> ReorderColumnsAsync(
    Guid projectId, ReorderColumnsRequest request, Guid userId);
        Task<ResponseBase> DeleteColumnAsync(Guid columnId, Guid userId);

    }
    public class BoardLogic : IBoardLogic
    {
        private readonly IBoardEndpointService _boardEndpointService;
        private readonly IProjectsEndpointService _projectsEndpointService;
        public BoardLogic(IBoardEndpointService boardEndpointService, IProjectsEndpointService projectsEndpointService)
        {
            _boardEndpointService = boardEndpointService;
            _projectsEndpointService = projectsEndpointService;
        }


        public async Task<BoardResponse> GetBoardAsync(
            Guid projectId, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                return new BoardResponse
                {
                    StatusCode = 404,
                    Message = "Project not found"
                };

            if (!await _projectsEndpointService.IsMemberAsync(projectId, userId))
                return new BoardResponse
                {
                    StatusCode = 403,
                    Message = "You are not a member of this project"
                };

            var columns = await _boardEndpointService.GetBoardColumnsAsync(projectId);

            return new BoardResponse
            {
                StatusCode = 200,
                Message = "Board retrieved successfully",
                ProjectId = projectId,
                Columns = columns.Select(c => new BoardColumnResponse
                {
                    ColumnId = c.ColumnId,
                    Name = c.Name,
                    Position = c.Position,
                    Tasks = c.Tasks
                        .OrderBy(t => t.Position)
                        .Select(t => new BoardTaskResponse
                        {
                            TaskId = t.TaskId,
                            Title = t.Title,
                            Description = t.Description,
                            Status = t.Status,
                            Position = t.Position,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt
                        }).ToList()
                }).ToList()
            };
        }


        public async Task<BoardColumnResponse> CreateColumnAsync(
            Guid projectId, CreateColumnRequest request, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (!await CanManageBoardAsync(projectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can manage columns");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required");

            var column = new BoardColumn
            {
                ProjectId = projectId,
                Name = request.Name.Trim(),
                Position = await _boardEndpointService.GetColumnCountAsync(projectId)
            };

            await _boardEndpointService.AddColumnAsync(column);

            return new BoardColumnResponse
            {
                ColumnId = column.ColumnId,
                Name = column.Name,
                Position = column.Position
            };
        }

        public async Task<BoardColumnResponse> RenameColumnAsync(
            Guid columnId, RenameColumnRequest request, Guid userId)
        {
            var column = await _boardEndpointService.GetColumnAsync(columnId);

            if (column == null)
                throw new KeyNotFoundException("Column not found");

            if (!await CanManageBoardAsync(column.ProjectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can rename columns");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required");

            column.Name = request.Name.Trim();

            await _projectsEndpointService.SaveChangesAsync();

            return new BoardColumnResponse
            {
                ColumnId = column.ColumnId,
                Name = column.Name,
                Position = column.Position
            };
        }

        public async Task<ResponseBase> ReorderColumnsAsync(
            Guid projectId, ReorderColumnsRequest request, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (!await CanManageBoardAsync(projectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can reorder columns");

            var columns = await _boardEndpointService.GetColumnsForReorderAsync(
                projectId);

            var requestedIds = request.ColumnIds;

            if (requestedIds.Distinct().Count() != requestedIds.Count ||
                requestedIds.Count != columns.Count ||
                !requestedIds.ToHashSet().SetEquals(
                    columns.Select(c => c.ColumnId)))
            {
                throw new ArgumentException(
                    "Provide every column ID belonging to this project exactly once");
            }

            var columnsById = columns.ToDictionary(c => c.ColumnId);

            for (int i = 0; i < requestedIds.Count; i++)
                columnsById[requestedIds[i]].Position = i;

            await _projectsEndpointService.SaveChangesAsync();

            return new ResponseBase
            {
                StatusCode = 200,
                Message = "Columns reordered successfully"
            };
        }

        public async Task<ResponseBase> DeleteColumnAsync(
            Guid columnId, Guid userId)
        {
            var column = await _boardEndpointService.GetColumnAsync(columnId);

            if (column == null)
                throw new KeyNotFoundException("Column not found");

            if (!await CanManageBoardAsync(column.ProjectId, userId))
                throw new UnauthorizedAccessException(
                    "Only the project owner or an admin can delete columns");

            if (column.Tasks.Any())
                throw new InvalidOperationException(
                    "Cannot delete a column that contains tasks. Move or delete its tasks first.");

            await _boardEndpointService.DeleteColumnAsync(column);

            return new ResponseBase
            {
                StatusCode = 200,
                Message = "Column deleted successfully"
            };
        }


        private async Task<bool> CanManageBoardAsync(
            Guid projectId, Guid userId)
        {
            var project = await _projectsEndpointService.GetProjectAsync(projectId);

            if (project == null)
                return false;

            var member = await _projectsEndpointService.GetProjectMemberAsync(
                projectId, userId);

            return member != null &&
                   (member.Role == "Owner" || member.Role == "Admin");
        }

    }
}
