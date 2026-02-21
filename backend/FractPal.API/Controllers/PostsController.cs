namespace FractPal.API.Controllers;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FractPal.Model.DTO.Post;
using FractPal.Service.Interface;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController(IPostService postService, ILikeService likeService) : ControllerBase
{
    private readonly IPostService postService = postService;
    private readonly ILikeService likeService = likeService;

    private Guid GetCurrentUserId() =>
        Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found"));

    private bool IsAdmin() => this.User.IsInRole("Admin");

    // GET: api/posts/feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var feed = await this.postService.GetFeedAsync(userId, page, pageSize);
            return this.Ok(feed);
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // GET: api/posts/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        try
        {
            var currentUserId = this.GetCurrentUserId();
            var post = await this.postService.GetPostByIdAsync(id, currentUserId);

            if (post == null)
                return this.NotFound(new { message = "Post not found" });

            return this.Ok(post);
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // GET: api/posts/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPosts()
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var posts = await this.postService.GetUserPostsAsync(userId, userId);
            return this.Ok(posts);
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // GET: api/posts/user/{userId}
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserPosts(Guid userId)
    {
        try
        {
            var currentUserId = this.GetCurrentUserId();
            var posts = await this.postService.GetUserPostsAsync(userId, currentUserId);
            return this.Ok(posts);
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // POST: api/posts/publish/{fractalId}
    [HttpPost("publish/{fractalId:guid}")]
    public async Task<IActionResult> PublishFractal(Guid fractalId, [FromBody] CreatePostRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var post = await this.postService.PublishFractalAsync(fractalId, userId, request);
            return this.CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (InvalidOperationException)
        {
            return this.Conflict();
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // POST: api/posts/unpublish/{fractalId}
    [HttpPost("unpublish/{fractalId:guid}")]
    public async Task<IActionResult> UnpublishFractal(Guid fractalId)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            await this.postService.UnpublishFractalAsync(fractalId, userId);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Post not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // PUT: api/posts/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var updated = await this.postService.UpdatePostAsync(id, userId, request);
            return this.Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Post not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // DELETE: api/posts/{id}
    // Admins can delete any post; regular users can only delete their own.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var isAdmin = this.IsAdmin();
            await this.postService.DeletePostAsync(id, userId, isAdmin);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Post not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    // POST: api/posts/{id}/like
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> ToggleLikePost(Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var isLiked = await this.likeService.ToggleLikePostAsync(id, userId);
            return this.Ok(new { isLiked });
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Post or fractal not found" });
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}
