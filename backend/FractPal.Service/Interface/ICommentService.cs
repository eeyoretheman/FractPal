namespace FractPal.Service.Interface;

using System;
using System.Collections.Generic;
using System.Text;
using FractPal.Model.DTO.Comment;

public interface ICommentService
{
    public Task<CommentDto> GetCommentById(Guid commentId);
    public Task<List<CommentDto>> GetPostComments(Guid postId);
    public Task<CommentDto> CreateComment(Guid userId, Guid postId, CreateCommentRequest request);
    public Task<CommentDto> UpdateComment(Guid userId, Guid commentId, UpdateCommentRequest request);
    public Task DeleteComment(Guid userId, Guid commentId);
}
