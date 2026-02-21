namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.User;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implements user profile and social features for FractPal, including profile
/// retrieval and updates, follow/unfollow toggling, and username search.
/// </summary>
public class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext context = context;

    /// <inheritdoc/>
    public async Task<UserProfileDto> GetProfileAsync(string userId, string currentUserId)
    {
        if (!Guid.TryParse(userId, out var userGuid) || !Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        var user = await this.context.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Fractals)
            .FirstOrDefaultAsync(u => u.Id == userGuid) ?? throw new KeyNotFoundException("User not found");

        var isFollowed = await this.context.Follows
            .AnyAsync(f => f.FollowerId == currentUserGuid && f.FollowingId == userGuid);

        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            JoinedDate = user.JoinedDate,
            Bio = user.Bio,
            ProfileImage = user.ProfilePicturePath,
            FollowerCount = user.Followers?.Count ?? 0,
            FollowingCount = user.Following?.Count ?? 0,
            FractalCount = user.Fractals?.Count ?? 0,
            IsFollowedByCurrentUser = isFollowed
        };
    }

    /// <inheritdoc/>
    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        var user = await this.context.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Fractals)
            .FirstOrDefaultAsync(u => u.Id == userGuid) ?? throw new KeyNotFoundException("User not found");

        user.Bio = request.Bio;
        await this.context.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            JoinedDate = user.JoinedDate,
            Bio = user.Bio,
            ProfileImage = user.ProfilePicturePath,
            FollowerCount = user.Followers?.Count ?? 0,
            FollowingCount = user.Following?.Count ?? 0,
            FractalCount = user.Fractals?.Count ?? 0,
            IsFollowedByCurrentUser = false
        };
    }

    /// <inheritdoc/>
    public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
    {
        if (!Guid.TryParse(followerId, out var followerGuid) || !Guid.TryParse(followingId, out var followingGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        if (followerGuid == followingGuid)
        {
            throw new InvalidOperationException("Cannot follow yourself");
        }

        var existingFollow = await this.context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerGuid && f.FollowingId == followingGuid);

        if (existingFollow != null)
        {
            this.context.Follows.Remove(existingFollow);
            await this.context.SaveChangesAsync();
            return false;
        }
        else
        {
            var follow = new Follow
            {
                FollowerId = followerGuid,
                FollowingId = followingGuid
            };
            this.context.Follows.Add(follow);
            await this.context.SaveChangesAsync();
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task<List<UserSearchDto>> SearchUsersAsync(string query, string currentUserId)
    {
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        var users = await this.context.Users
            .Where(u => u.UserName!.Contains(query))
            .Include(u => u.Followers)
            .Take(20)
            .ToListAsync();

        var dtos = new List<UserSearchDto>();

        foreach (var user in users)
        {
            var isFollowed = await this.context.Follows
                .AnyAsync(f => f.FollowerId == currentUserGuid && f.FollowingId == user.Id);

            dtos.Add(new UserSearchDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName ?? "",
                ProfileImage = user.ProfilePicturePath,
                FollowerCount = user.Followers?.Count ?? 0,
                IsFollowedByCurrentUser = isFollowed
            });
        }

        return dtos;
    }
}
