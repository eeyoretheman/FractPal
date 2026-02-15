namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.Fractal;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FractalController : ControllerBase
{
    private readonly IFractalService _fractalService;

    public FractalController(IFractalService fractalService)
    {
        _fractalService = fractalService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // GET: api/fractal/feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feed = await _fractalService.GetFeedAsync(userId, page, pageSize);
            return Ok(feed);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyFractals()
    {
        try
        {
            var userId = GetCurrentUserId();
            var fractals = await _fractalService.GetUserFractalsAsync(userId, userId);
            return Ok(fractals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFractals(string userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var fractals = await _fractalService.GetPublishedFractalsByUserAsync(userId, currentUserId);
            return Ok(fractals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/fractal/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFractalById(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.GetFractalByIdAsync(id, userId);

            if (fractal == null)
            {
                return NotFound(new { message = "Fractal not found" });
            }

            return Ok(fractal);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal
    [HttpPost]
    public async Task<IActionResult> CreateFractal([FromBody] CreateFractalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.CreateFractalAsync(userId, request);
            return CreatedAtAction(nameof(GetFractalById), new { id = fractal.Id }, fractal);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/fractal/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFractal(string id, [FromBody] UpdateFractalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.UpdateFractalAsync(id, userId, request);
            return Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/fractal/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFractal(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _fractalService.DeleteFractalAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/publish
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishFractal(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.PublishFractalAsync(id, userId);
            return Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/unpublish
    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> UnpublishFractal(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.UnpublishFractalAsync(id, userId);
            return Ok(fractal);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Fractal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/fork
    [HttpPost("{id}/fork")]
    public async Task<IActionResult> ForkFractal(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var fractal = await _fractalService.ForkFractalAsync(id, userId);
            return CreatedAtAction(nameof(GetFractalById), new { id = fractal.Id }, fractal);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Fractal not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/fractal/{id}/like
    [HttpPost("{id}/like")]
    public async Task<IActionResult> ToggleLike(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isLiked = await _fractalService.ToggleLikeAsync(id, userId);
            return Ok(new { isLiked });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
