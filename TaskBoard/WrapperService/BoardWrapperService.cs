using TaskBoard.LogicService;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;

namespace TaskBoard.WrapperService
{
    public interface IBoardWrapperService
    {
        Task<BoardResponse> GetBoardAsync(Guid projectId, Guid userId);
        Task<BoardColumnResponse> CreateColumnAsync(Guid projectId, CreateColumnRequest request, Guid userId);
        Task<BoardColumnResponse> RenameColumnAsync(Guid columnId, RenameColumnRequest request, Guid userId);
        Task<ResponseBase> ReorderColumnsAsync(Guid projectId, ReorderColumnsRequest request, Guid userId);
        Task<ResponseBase> DeleteColumnAsync(Guid columnId, Guid userId);
    }
    public class BoardWrapperService : IBoardWrapperService
    {
        private readonly IBoardLogic _boardLogic;
        public BoardWrapperService(IBoardLogic boardLogic)
        {
            _boardLogic = boardLogic;
        }


        public Task<BoardResponse> GetBoardAsync(Guid projectId, Guid userId) =>
            _boardLogic.GetBoardAsync(projectId, userId);

        public Task<BoardColumnResponse> CreateColumnAsync(
            Guid projectId, CreateColumnRequest request, Guid userId) =>
            _boardLogic.CreateColumnAsync(projectId, request, userId);

        public Task<BoardColumnResponse> RenameColumnAsync(
            Guid columnId, RenameColumnRequest request, Guid userId) =>
            _boardLogic.RenameColumnAsync(columnId, request, userId);

        public Task<ResponseBase> ReorderColumnsAsync(
            Guid projectId, ReorderColumnsRequest request, Guid userId) =>
            _boardLogic.ReorderColumnsAsync(projectId, request, userId);

        public Task<ResponseBase> DeleteColumnAsync(Guid columnId, Guid userId) =>
            _boardLogic.DeleteColumnAsync(columnId, userId);

    }
}
