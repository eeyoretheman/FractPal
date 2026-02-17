namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.User;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService userService = userService;

    private string GetCurrentUserId() => this.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");

    // GET: api/user/profile
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

    // GET: api/user/{id}
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

    // PUT: api/user/profile
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

    // POST: api/user/{id}/follow
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

    // GET: api/user/search
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
