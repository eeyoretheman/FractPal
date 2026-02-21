using System.Security.Claims;
using FractPal.Model.DTO.Comment;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FractPal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService commentService = commentService;

    private Guid GetCurrentUserId() =>
        Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found"));

    // GET: api/comments/{id}
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

    // GET: api/comments/post/{postId}
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

    // POST: api/comments/{postId}
    [HttpPost("{postId:guid}")]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

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

    // PUT: api/comments/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

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

    // DELETE: api/comments/{id}
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
