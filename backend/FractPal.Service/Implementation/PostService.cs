namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.Post;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class PostService(ApplicationDbContext dbContext) : IPostService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<PostFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var query = context.Posts
            .Include(p => p.Author)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Likes)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Author)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PostFeedResponse
        {
            Posts = posts.Select(p => MapToDto(p, userId)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid postId, Guid currentUserId)
    {
        var post = await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Likes)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Author)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId);

        return post == null ? null : MapToDto(post, currentUserId);
    }

    public async Task<List<PostDto>> GetUserPostsAsync(Guid userId, Guid currentUserId)
    {
        var posts = await context.Posts
            .Where(p => p.AuthorId == userId)
            .Include(p => p.Author)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Likes)
            .Include(p => p.Fractal)
                .ThenInclude(f => f!.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p => MapToDto(p, currentUserId)).ToList();
    }

    public async Task<PostDto> PublishFractalAsync(Guid fractalId, Guid userId, CreatePostRequest request)
    {
        var fractal = await context.Fractals.FirstOrDefaultAsync(f => f.Id == fractalId)
            ?? throw new KeyNotFoundException("Fractal not found");

        if (fractal.AuthorId != userId)
            throw new UnauthorizedAccessException("You don't have permission to publish this fractal");

        var post = new Post
        {
            Id = Guid.NewGuid(),
            FractalId = fractalId,
            AuthorId = userId,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        await context.Entry(post).Reference(p => p.Author).LoadAsync();
        await context.Entry(post).Reference(p => p.Fractal).LoadAsync();
        await context.Entry(post.Fractal!).Collection(f => f.Likes).LoadAsync();

        return MapToDto(post, userId);
    }

    public async Task UnpublishFractalAsync(Guid fractalId, Guid userId)
    {
        var post = await context.Posts
            .FirstOrDefaultAsync(p => p.FractalId == fractalId && p.AuthorId == userId)
                ?? throw new KeyNotFoundException("Post not found");

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
    }

    public async Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request)
    {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new KeyNotFoundException("Post not found");

        if (post.AuthorId != userId)
            throw new UnauthorizedAccessException("You don't have permission to update this post");

        post.Name = request.Name;
        post.Description = request.Description;
        post.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        await context.Entry(post).Reference(p => p.Author).LoadAsync();
        await context.Entry(post).Reference(p => p.Fractal).LoadAsync();
        if (post.Fractal != null)
            await context.Entry(post.Fractal).Collection(f => f.Likes).LoadAsync();

        return MapToDto(post, userId);
    }

    public async Task DeletePostAsync(Guid postId, Guid userId, bool isAdmin = false)
    {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new KeyNotFoundException("Post not found");

        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("You don't have permission to delete this post");

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
    }

    private static PostDto MapToDto(Post post, Guid currentUserId) => new()
    {
        Id = post.Id,
        FractalId = post.FractalId,
        AuthorId = post.AuthorId,
        Username = post.Author?.UserName ?? "",
        Name = post.Name,
        Description = post.Description,
        Thumbnail = post.Fractal?.Thumbnail,
        LikeCount = post.Fractal?.Likes?.Count ?? 0,
        IsLikedByCurrentUser = post.Fractal?.Likes?.Any(l => l.UserId == currentUserId) ?? false,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt
    };
}
