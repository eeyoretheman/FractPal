namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.User;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext context = context;

    public async Task<UserProfileDto> GetProfileAsync(string userId, string currentUserId)
    {
        var user = await this.context.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Fractals)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found");

        var isFollowed = await this.context.Follows
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            JoinedDate = user.JoinedDate,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            FollowerCount = user.Followers.Count,
            FollowingCount = user.Following.Count,
            FractalCount = user.Fractals.Count(f => f.IsPublished),
            IsFollowedByCurrentUser = isFollowed
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await this.context.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Fractals)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found");

        user.Bio = request.Bio;
        await this.context.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            JoinedDate = user.JoinedDate,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            FollowerCount = user.Followers.Count,
            FollowingCount = user.Following.Count,
            FractalCount = user.Fractals.Count(f => f.IsPublished),
            IsFollowedByCurrentUser = false
        };
    }

    public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("Cannot follow yourself");
        }

        var existingFollow = await this.context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (existingFollow != null)
        {
            // Unfollow
            this.context.Follows.Remove(existingFollow);
            await this.context.SaveChangesAsync();
            return false;
        }
        else
        {
            // Follow
            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId
            };
            this.context.Follows.Add(follow);
            await this.context.SaveChangesAsync();
            return true;
        }
    }

    public async Task<List<UserSearchDto>> SearchUsersAsync(string query, string currentUserId)
    {
        var users = await this.context.Users
            .Where(u => u.UserName!.Contains(query))
            .Include(u => u.Followers)
            .Take(20)
            .ToListAsync();

        var dtos = new List<UserSearchDto>();

        foreach (var user in users)
        {
            var isFollowed = await this.context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);

            dtos.Add(new UserSearchDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                ProfileImageUrl = user.ProfileImageUrl,
                FollowerCount = user.Followers.Count,
                IsFollowedByCurrentUser = isFollowed
            });
        }

        return dtos;
    }
}
