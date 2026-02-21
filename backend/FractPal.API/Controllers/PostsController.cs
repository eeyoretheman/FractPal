namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.Post;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles post lifecycle operations including feed retrieval, publishing and unpublishing
/// fractals as posts, and social interactions (likes). All endpoints require an authenticated user.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController(IPostService postService, ILikeService likeService) : ControllerBase
{
    private readonly IPostService postService = postService;
    private readonly ILikeService likeService = likeService;

    /// <summary>
    /// Retrieves the authenticated user's ID from the JWT claims.
    /// </summary>
    /// <returns>The current user's ID as a <see cref="Guid"/>.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is missing or not a valid GUID.</exception>
    private Guid GetCurrentUserId() =>
        Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found"));

    /// <summary>
    /// Determines whether the authenticated user has the Admin role.
    /// </summary>
    /// <returns><c>true</c> if the user is an admin; otherwise <c>false</c>.</returns>
    private bool IsAdmin() => this.User.IsInRole("Admin");

    /// <summary>
    /// Returns a paginated feed of all posts, ordered by most recently created.
    /// </summary>
    /// <param name="page">The 1-based page number. Defaults to 1.</param>
    /// <param name="pageSize">The number of posts per page. Defaults to 20.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="PostFeedResponse"/> containing
    /// the posts and pagination metadata;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Retrieves a single post by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="PostDto"/> on success;
    /// <see cref="NotFoundObjectResult"/> if no post exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Returns all posts belonging to the authenticated user.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="PostDto"/>;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Returns all posts belonging to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose posts to retrieve.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="PostDto"/>;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Publishes a fractal as a post. Only the fractal's owner may publish it.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal to publish.</param>
    /// <param name="request">The post metadata including name and description.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> pointing to <see cref="GetPostById"/> with the new <see cref="PostDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// <see cref="NotFoundObjectResult"/> if no fractal exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the fractal's owner;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Removes the published post associated with a fractal, effectively unpublishing it.
    /// Only the post's author may unpublish it.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal whose post to remove.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on successful removal;
    /// <see cref="NotFoundObjectResult"/> if no matching post exists;
    /// <see cref="ForbidResult"/> if the authenticated user is not the post's author;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Updates the name and description of an existing post. Only the post's author may update it.
    /// </summary>
    /// <param name="id">The unique identifier of the post to update.</param>
    /// <param name="request">The updated post metadata.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with the updated <see cref="PostDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// <see cref="NotFoundObjectResult"/> if no post exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the post's author;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Permanently deletes a post. Admins may delete any post; regular users may only delete their own.
    /// </summary>
    /// <param name="id">The unique identifier of the post to delete.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on successful deletion;
    /// <see cref="NotFoundObjectResult"/> if no post exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the post's author and is not an admin;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Toggles the like state on a post for the authenticated user. Resolves to the
    /// underlying fractal's like collection. Calling this when already liked will unlike,
    /// and vice versa.
    /// </summary>
    /// <param name="id">The unique identifier of the post to like or unlike.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with an <c>isLiked</c> boolean indicating the new state;
    /// <see cref="NotFoundObjectResult"/> if the post or its underlying fractal is not found;
    /// 500 if an unexpected error occurs.
    /// </returns>
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
