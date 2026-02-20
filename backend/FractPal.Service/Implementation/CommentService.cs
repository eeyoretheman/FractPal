namespace FractPal.Service.Implementation;

using System;
using System.Collections.Generic;
using System.Text;
using FractPal.Data;
using FractPal.Model.DTO.Comment;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class CommentService(ApplicationDbContext dbContext) : ICommentService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<CommentDto> CreateComment(Guid userId, CreateCommentRequest request)
    {
        var comment = new Comment
        {
            AuthorId = userId,
            Content = request.Content,
            PostId = request.PostId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        this.context.Comments.Add(comment);
        await this.context.SaveChangesAsync();

        return MapToDto(comment);
    }
    public async Task DeleteComment(Guid userId, Guid commentId)
    {
        var comment = await this.context.Comments
            .FindAsync(commentId) ?? throw new KeyNotFoundException("Comment not found");

        if (userId != comment.AuthorId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this comment");
        }

        this.context.Comments.Remove(comment);
        await this.context.SaveChangesAsync();
    }
    public async Task<CommentDto> GetCommentById(Guid commentId) =>
        MapToDto(await this.context.Comments
            .FindAsync(commentId) ?? throw new KeyNotFoundException("Comment not found"));
    public async Task<List<CommentDto>> GetPostComments(Guid postId) =>
        await this.context.Comments
                          .Where(c => c.PostId == postId)
                          .Select(c => MapToDto(c))
                          .ToListAsync();
    public async Task<CommentDto> UpdateComment(Guid userId, Guid commentId, UpdateCommentRequest request)
    {
        var comment = await this.context.Comments
            .FindAsync(commentId) ?? throw new KeyNotFoundException("Comment not found");

        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You cannot edit this comment");
        }

        comment.UpdatedAt = DateTime.UtcNow;
        comment.Content = request.Content;

        await this.context.SaveChangesAsync();

        return MapToDto(comment);
    }


    private static CommentDto MapToDto(Comment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        AuthorId = comment.AuthorId,
        PostId = comment.PostId
    };
}
