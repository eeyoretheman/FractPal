namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.Comment;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class CommentService(ApplicationDbContext context) : ICommentService
{
    private readonly ApplicationDbContext context = context;

    public async Task<CommentDto> CreateCommentAsync(string fractalId, string userId, CreateCommentRequest request)
    {
        var fractal = await this.context.Fractals.FirstOrDefaultAsync(f => f.Id == fractalId)
            ?? throw new KeyNotFoundException("Fractal not found");

        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found");

        var comment = new Comment
        {
            FractalId = fractalId,
            UserId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        this.context.Comments.Add(comment);
        await this.context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            FractalId = comment.FractalId,
            UserId = comment.UserId,
            Username = user.UserName ?? "",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByFractalIdAsync(string fractalId)
    {
        var comments = await this.context.Comments
            .Where(c => c.FractalId == fractalId)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new CommentDto
        {
            Id = c.Id,
            FractalId = c.FractalId,
            UserId = c.UserId,
            Username = c.User.UserName ?? "",
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<CommentDto?> GetCommentByIdAsync(string commentId)
    {
        var comment = await this.context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return null;
        }

        return new CommentDto
        {
            Id = comment.Id,
            FractalId = comment.FractalId,
            UserId = comment.UserId,
            Username = comment.User.UserName ?? "",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public async Task<CommentDto> UpdateCommentAsync(string commentId, string userId, UpdateCommentRequest request)
    {
        var comment = await this.context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new KeyNotFoundException("Comment not found");

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own comments");
        }

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            FractalId = comment.FractalId,
            UserId = comment.UserId,
            Username = comment.User.UserName ?? "",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public async Task DeleteCommentAsync(string commentId, string userId)
    {
        var comment = await this.context.Comments.FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new KeyNotFoundException("Comment not found");

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments");
        }

        this.context.Comments.Remove(comment);
        await this.context.SaveChangesAsync();
    }
}
