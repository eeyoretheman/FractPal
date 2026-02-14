namespace FractPal.API.Controllers;

using System.Security.Claims;
using FractPal.Model.DTO.User;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // GET: api/user/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId, userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserProfile(string id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(id, currentUserId);
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/user/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/user/{id}/follow
    [HttpPost("{id}/follow")]
    public async Task<IActionResult> ToggleFollow(string id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isFollowing = await _userService.ToggleFollowAsync(currentUserId, id);
            return Ok(new { isFollowing });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/user/search
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "Query parameter is required" });
        }

        try
        {
            var currentUserId = GetCurrentUserId();
            var users = await _userService.SearchUsersAsync(query, currentUserId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
