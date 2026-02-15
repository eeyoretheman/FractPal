namespace FractPal.Service.Interface;

using FractPal.Model.DTO.User;

public interface IUserService
{
    public Task<UserProfileDto> GetProfileAsync(string userId, string currentUserId);
    public Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    public Task<bool> ToggleFollowAsync(string followerId, string followingId);
    public Task<List<UserSearchDto>> SearchUsersAsync(string query, string currentUserId);
}
