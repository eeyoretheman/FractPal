namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.User;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles user profile and social operations such as viewing profiles,
/// updating bio, following/unfollowing users, and searching for users.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService userService = userService;

    /// <summary>
    /// Retrieves the authenticated user's ID from the JWT claims.
    /// </summary>
    /// <returns>The current user's ID as a string.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is missing.</exception>
    private string GetCurrentUserId() => this.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User ID not found");

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="UserProfileDto"/> on success;
    /// <see cref="NotFoundObjectResult"/> if the user no longer exists;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var profile = await this.userService.GetProfileAsync(userId, userId);
            return this.Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves the public profile of any user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user whose profile to retrieve.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="UserProfileDto"/> on success;
    /// <see cref="NotFoundObjectResult"/> if no user exists with the given ID;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserProfile(string id)
    {
        try
        {
            var currentUserId = this.GetCurrentUserId();
            var profile = await this.userService.GetProfileAsync(id, currentUserId);
            return this.Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the authenticated user's profile information (e.g. bio).
    /// </summary>
    /// <param name="request">The updated profile data.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with the updated <see cref="UserProfileDto"/> on success;
    /// <see cref="BadRequestObjectResult"/> if the request body fails validation;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var userId = this.GetCurrentUserId();
            var profile = await this.userService.UpdateProfileAsync(userId, request);
            return this.Ok(profile);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toggles the follow relationship between the authenticated user and the target user.
    /// Calling this endpoint when already following will unfollow, and vice versa.
    /// </summary>
    /// <param name="id">The ID of the user to follow or unfollow.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with an <c>isFollowing</c> boolean indicating the new state;
    /// <see cref="BadRequestObjectResult"/> if attempting to follow oneself;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpPost("{id}/follow")]
    public async Task<IActionResult> ToggleFollow(string id)
    {
        try
        {
            var currentUserId = this.GetCurrentUserId();
            var isFollowing = await this.userService.ToggleFollowAsync(currentUserId, id);
            return this.Ok(new { isFollowing });
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Searches for users whose usernames contain the given query string.
    /// </summary>
    /// <param name="query">The search term to match against usernames. Must not be empty.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a list of <see cref="UserSearchDto"/> on success;
    /// <see cref="BadRequestObjectResult"/> if the query parameter is empty or whitespace;
    /// 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return this.BadRequest(new { message = "Query parameter is required" });
        }

        try
        {
            var currentUserId = this.GetCurrentUserId();
            var users = await this.userService.SearchUsersAsync(query, currentUserId);
            return this.Ok(users);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = ex.Message });
        }
    }
}
