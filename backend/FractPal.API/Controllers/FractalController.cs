namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.Fractal;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles all fractal CRUD operations, feed retrieval, forking, and social
/// interactions (likes). All endpoints require an authenticated user.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FractalController(IFractalService fractalService, ILikeService likeService) : ControllerBase
{
    private readonly IFractalService fractalService = fractalService;
    private readonly ILikeService likeService = likeService;

    /// <summary>
    /// Retrieves the authenticated user's ID from the JWT claims.
    /// </summary>
    /// <returns>The current user's ID as a <see cref="Guid"/>.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is missing or not a valid GUID.</exception>
    private Guid GetCurrentUserId() => Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found"));

    /// <summary>
    /// Returns a paginated feed of all published fractals, ordered by most recently published.
    /// </summary>
    /// <param name="page">The 1-based page number. Defaults to 1.</param>
    /// <param name="pageSize">The number of fractals per page. Defaults to 20.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="FractalFeedResponse"/> containing
    /// the fractals and pagination metadata;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Returns all fractals (published and draft) belonging to the authenticated user.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="FractalDto"/>;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Returns all published fractals belonging to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose published fractals to retrieve.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="FractalDto"/>;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFractals(Guid userId)
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

    /// <summary>
    /// Retrieves a single fractal by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the fractal.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="FractalDto"/> on success;
    /// <see cref="NotFoundObjectResult"/> if no fractal exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFractalById(Guid id)
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

    /// <summary>
    /// Creates a new fractal owned by the authenticated user. The fractal starts as a draft
    /// and must be explicitly published via <see cref="PublishFractal"/>.
    /// </summary>
    /// <param name="request">The fractal configuration data including L-system rules and optional thumbnail.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> pointing to <see cref="GetFractalById"/> with the new <see cref="FractalDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// 500 if an unexpected error occurs.
    /// </returns>
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

    /// <summary>
    /// Updates an existing fractal. If the authenticated user is not the owner, a forked
    /// copy is created instead and the original is left unchanged.
    /// </summary>
    /// <param name="id">The unique identifier of the fractal to update.</param>
    /// <param name="request">The updated fractal configuration data.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with the updated or forked <see cref="FractalDto"/>;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// <see cref="NotFoundObjectResult"/> if no fractal exists with the given ID;
    /// <see cref="ForbidResult"/> if the user lacks permission (owner check is done in service);
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFractal(Guid id, [FromBody] UpdateFractalRequest request)
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

    /// <summary>
    /// Permanently deletes a fractal. Only the owner may delete their own fractal.
    /// </summary>
    /// <param name="id">The unique identifier of the fractal to delete.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on successful deletion;
    /// <see cref="NotFoundObjectResult"/> if no fractal exists with the given ID;
    /// <see cref="ForbidResult"/> if the authenticated user is not the owner;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFractal(Guid id)
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

    /// <summary>
    /// Creates a copy (fork) of an existing published fractal under the authenticated user's account.
    /// The forked fractal starts as a draft.
    /// </summary>
    /// <param name="id">The unique identifier of the fractal to fork.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> pointing to <see cref="GetFractalById"/> with the new forked <see cref="FractalDto"/>;
    /// <see cref="NotFoundObjectResult"/> if no fractal exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPost("{id}/fork")]
    public async Task<IActionResult> ForkFractal(Guid id)
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

    //// POST: api/fractal/{id}/like
    //[HttpPost("{id}/like")]
    //public async Task<IActionResult> ToggleLike(Guid id)
    //{
    //    try
    //    {
    //        var userId = this.GetCurrentUserId();
    //        var isLiked = await this.likeService.ToggleLikeAsync(id, userId);
    //        return this.Ok(new { isLiked });
    //    }
    //    catch (KeyNotFoundException)
    //    {
    //        return this.NotFound(new { message = "Fractal not found" });
    //    }
    //    catch (Exception)
    //    {
    //        return this.StatusCode(500, new { message = "An unexpected error occurred" });
    //    }
    //}
}
