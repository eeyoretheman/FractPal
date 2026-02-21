namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implements like/unlike toggle logic for fractals and posts.
/// Likes are stored at the fractal level; post likes resolve to the
/// underlying fractal.
/// </summary>
public class LikeService(ApplicationDbContext dbContext) : ILikeService
{
    private readonly ApplicationDbContext context = dbContext;

    /// <inheritdoc/>
    public async Task<bool> ToggleLikeAsync(Guid fractalId, Guid userId)
    {
        var fractalExists = await this.context.Fractals.AnyAsync(f => f.Id == fractalId);
        if (!fractalExists)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        var existingLike = await this.context.Likes
            .FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);

        if (existingLike != null)
        {
            this.context.Likes.Remove(existingLike);
            await this.context.SaveChangesAsync();
            return false;
        }
        else
        {
            var like = new Like
            {
                FractalId = fractalId,
                UserId = userId
            };

            this.context.Likes.Add(like);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Concurrent like from another request - treat as already liked.
                return true;
            }

            return true;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ToggleLikePostAsync(Guid postId, Guid userId)
    {
        var post = await this.context.Posts
            .FindAsync(postId) ?? throw new KeyNotFoundException("Post not found");

        return await this.ToggleLikeAsync(post.FractalId, userId);
    }
}
