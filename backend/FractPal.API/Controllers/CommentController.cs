namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.Comment;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService commentService = commentService;

    private string GetCurrentUserId() => this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");

    // GET: api/comment/fractal/{fractalId}
    [HttpGet("fractal/{fractalId}")]
    public async Task<IActionResult> GetCommentsByFractal(string fractalId)
    {
        try
        {
            var comments = await this.commentService.GetCommentsByFractalIdAsync(fractalId);
            return this.Ok(comments);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/comment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCommentById(string id)
    {
        try
        {
            var comment = await this.commentService.GetCommentByIdAsync(id);
            if (comment == null)
                return this.NotFound(new { message = "Comment not found" });
            return this.Ok(comment);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/comment/{fractalId}
    [HttpPost("{fractalId}")]
    public async Task<IActionResult> CreateComment(string fractalId, [FromBody] CreateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var comment = await this.commentService.CreateCommentAsync(fractalId, userId, request);
            return this.CreatedAtAction(nameof(this.GetCommentById), new { id = comment.Id }, comment);
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/comment/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(string id, [FromBody] UpdateCommentRequest request)
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var comment = await this.commentService.UpdateCommentAsync(id, userId, request);
            return this.Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/comment/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            await this.commentService.DeleteCommentAsync(id, userId);
            return this.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }
}
