namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Comment;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(string fractalId, string userId, CreateCommentRequest request);
    Task<IEnumerable<CommentDto>> GetCommentsByFractalIdAsync(string fractalId);
    Task<CommentDto?> GetCommentByIdAsync(string commentId);
    Task<CommentDto> UpdateCommentAsync(string commentId, string userId, UpdateCommentRequest request);
    Task DeleteCommentAsync(string commentId, string userId);
}
