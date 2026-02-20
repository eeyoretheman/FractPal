namespace FractPal.Service.Implementation;

using System;
using System.Collections.Generic;
using System.Text;
using FractPal.Data;
using FractPal.Model.DTO.Fractal;
using FractPal.Model.DTO.Post;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class PostService(ApplicationDbContext dbContext) : IPostService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<PostFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var query = this.context.Posts
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = posts.Select(MapToDto).ToList();

        return new PostFeedResponse
        {
            Posts = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    public async Task<PostDto?> GetPostByIdAsync(Guid postId, Guid currentUserId) =>
        MapToDto(await this.context.Posts
            .Include(p => p.Author)
            .Include(p => p.Fractal)
            .FirstOrDefaultAsync(p => p.Id == postId) ?? throw new KeyNotFoundException("Post not found"));
    public async Task<List<PostDto>> GetUserPostsAsync(Guid userId)
    {
        var posts = await this.context.Posts
                          .Where(p => p.AuthorId == userId)
                          .OrderByDescending(p => p.CreatedAt)
                          .ToListAsync();

        return [.. posts.Select(MapToDto)];
    }
    
    public async Task<PostDto> PublishFractalAsync(Guid fractalId, Guid userId, CreatePostRequest request)
    {
        var fractal = await this.context.Fractals
            .FirstOrDefaultAsync(f => f.Id == fractalId) ?? throw new KeyNotFoundException("Fractal not found");

        if (fractal.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to publish this fractal");
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            FractalId = fractalId,
            AuthorId = userId,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        this.context.Posts.Add(post);
        await this.context.SaveChangesAsync();

        return MapToDto(post);
    }
    public async Task UnpublishFractalAsync(Guid fractalId, Guid userId)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.FractalId == fractalId && p.AuthorId == userId)
                ?? throw new KeyNotFoundException("Post not found");

        if (post.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to unpublish this fractal");
        }

        this.context.Remove(post);
        await this.context.SaveChangesAsync();
    }
    public async Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId) ?? throw new KeyNotFoundException("Post not found");

        if (post.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this post");
        }

        post.Name = request.Name;
        post.Description = request.Description;
        post.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task DeletePostAsync(Guid postId, Guid userId)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId) ?? throw new KeyNotFoundException("Post not found");

        if (post.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this post");
        }

        this.context.Posts.Remove(post);
        await this.context.SaveChangesAsync();
    }


    private static PostDto MapToDto(Post post) => new()
    {
        Id = post.Id,
        FractalId = post.FractalId,
        AuthorId = post.AuthorId,
        Name = post.Name,
        Description = post.Description,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
        LikeCount = post.Likes.Count,
    };
}
