namespace FractPal.Service.Interface;

using FractPal.Model.DTO.User;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId, string currentUserId);
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<bool> ToggleFollowAsync(string followerId, string followingId);
    Task<List<UserSearchDto>> SearchUsersAsync(string query, string currentUserId);
}
