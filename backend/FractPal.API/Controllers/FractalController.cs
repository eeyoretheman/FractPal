namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.Fractal;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FractalController(IFractalService fractalService) : ControllerBase
{
    private readonly IFractalService fractalService = fractalService;

    private string GetCurrentUserId() => this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");

    // GET: api/fractal/feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var feed = await this.fractalService.GetFeedAsync(userId, page, pageSize);
            return this.Ok(feed);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyFractals()
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var fractals = await this.fractalService.GetUserFractalsAsync(userId, userId);
            return this.Ok(fractals);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFractals(string userId)
    {
        try
        {
            var currentUserId = this.GetCurrentUserId();
            var fractals = await this.fractalService.GetPublishedFractalsByUserAsync(userId, currentUserId);
            return this.Ok(fractals);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFractalById(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.GetFractalByIdAsync(id, userId);

            if (fractal == null)
            {
                return this.NotFound(new { message = "Fractal not found" });
            }

            return this.Ok(fractal);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal
    [HttpPost]
    public async Task<IActionResult> CreateFractal([FromBody] CreateFractalRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.CreateFractalAsync(userId, request);
            return this.CreatedAtAction(nameof(GetFractalById), new { id = fractal.Id }, fractal);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/fractal/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFractal(string id, [FromBody] UpdateFractalRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.UpdateFractalAsync(id, userId, request);
            return this.Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/fractal/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFractal(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            await this.fractalService.DeleteFractalAsync(id, userId);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/publish
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishFractal(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.PublishFractalAsync(id, userId);
            return this.Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/unpublish
    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> UnpublishFractal(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.UnpublishFractalAsync(id, userId);
            return this.Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/fork
    [HttpPost("{id}/fork")]
    public async Task<IActionResult> ForkFractal(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var fractal = await this.fractalService.ForkFractalAsync(id, userId);
            return this.CreatedAtAction(nameof(GetFractalById), new { id = fractal.Id }, fractal);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "Fractal not found" });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/like
    [HttpPost("{id}/like")]
    public async Task<IActionResult> ToggleLike(string id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var isLiked = await this.fractalService.ToggleLikeAsync(id, userId);
            return this.Ok(new { isLiked });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }
}
