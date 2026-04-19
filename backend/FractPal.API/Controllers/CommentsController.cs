namespace FractPal.API.Controllers;

using FractPal.Service.Interface;
using FractPal.Model.DTO.Comment;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles comment CRUD operations on posts. All endpoints require an authenticated user.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService commentService = commentService;

    /// <summary>
    /// Retrieves the authenticated user's ID from the JWT claims.
    /// </summary>
    /// <returns>The current user's ID as a <see cref="Guid"/>.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is missing or not a valid GUID.</exception>
    private Guid GetCurrentUserId() =>
        Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found"));

    /// <summary>
    /// Retrieves a single comment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the comment.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="CommentDto"/> on success;
    /// <see cref="NotFoundObjectResult"/> if no comment exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommentById(Guid id)
    {
        try
        {
            var dto = await this.commentService.GetCommentById(id);
            return this.Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Comment not found" });
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Retrieves all comments for a given post, ordered by creation time (oldest first).
    /// </summary>
    /// <param name="postId">The unique identifier of the post whose comments to retrieve.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="CommentDto"/>;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("post/{postId:guid}")]
    public async Task<IActionResult> GetPostComments(Guid postId)
    {
        try
        {
            var dtos = await this.commentService.GetPostComments(postId);
            return this.Ok(dtos);
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Creates a new comment on the specified post by the authenticated user.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to comment on.</param>
    /// <param name="request">The comment content.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> pointing to <see cref="GetCommentById"/> with the new <see cref="CommentDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// <see cref="NotFoundObjectResult"/> if no post exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPost("{postId:guid}")]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }
        try
        {
            var userId = this.GetCurrentUserId();
            var dto = await this.commentService.CreateComment(userId, postId, request);
            return this.CreatedAtAction(nameof(GetCommentById), new { id = dto.Id }, dto);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Post not found" });
        }
        catch (Exception)
        {
            return this.StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Updates the content of an existing comment. Only the comment's author may edit it.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to update.</param>
    /// <param name="request">The updated comment content.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with the updated <see cref="CommentDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// <see cref="NotFoundObjectResult"/> if no comment exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the comment's author;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }
        try
        {
            var userId = this.GetCurrentUserId();
            var dto = await this.commentService.UpdateComment(userId, id, request);
            return this.Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Comment not found" });
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
    /// Permanently deletes a comment. Only the comment's author may delete it.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to delete.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on successful deletion;
    /// <see cref="NotFoundObjectResult"/> if no comment exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the comment's author;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            await this.commentService.DeleteComment(userId, id);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Comment not found" });
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
}
