namespace FractPal.Service.Implementation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractPal.Data;
using FractPal.Model.DTO.Comment;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class CommentService(ApplicationDbContext dbContext) : ICommentService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<CommentDto> CreateComment(Guid userId, Guid postId, CreateCommentRequest request)
    {
        if(await this.context.Posts.FindAsync(postId) == null)
        {
            throw new KeyNotFoundException("Post not found");
        }

        var comment = new Comment
        {
            AuthorId = userId,
            Content = request.Content,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        this.context.Comments.Add(comment);
        await this.context.SaveChangesAsync();

        // Load author for DTO username
        await this.context.Entry(comment).Reference(c => c.Author).LoadAsync();

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

    public async Task<CommentDto> GetCommentById(Guid commentId)
    {
        var comment = await this.context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new KeyNotFoundException("Comment not found");

        return MapToDto(comment);
    }

    public async Task<List<CommentDto>> GetPostComments(Guid postId)
    {
        var comments = await this.context.Comments
            .Include(c => c.Author)
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToDto).ToList();
    }

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

        // Ensure author is loaded for mapping
        await this.context.Entry(comment).Reference(c => c.Author).LoadAsync();

        return MapToDto(comment);
    }

    private static CommentDto MapToDto(Comment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        AuthorId = comment.AuthorId,
        PostId = comment.PostId,
        Username = comment.Author?.UserName ?? string.Empty
    };
}
