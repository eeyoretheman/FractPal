namespace FractPal.Service.Interface;

using FractPal.Model.DTO.User;

/// <summary>
/// Provides user profile management and social features such as following and search.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves the profile of a user by their ID.
    /// </summary>
    /// <param name="userId">The ID of the user whose profile to retrieve.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsFollowedByCurrentUser</c>.</param>
    /// <returns>A <see cref="UserProfileDto"/> for the requested user.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no user exists with <paramref name="userId"/>.</exception>
    public Task<UserProfileDto> GetProfileAsync(string userId, string currentUserId);

    /// <summary>
    /// Updates mutable profile fields (e.g. bio) for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="request">The updated profile data.</param>
    /// <returns>The updated <see cref="UserProfileDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no user exists with <paramref name="userId"/>.</exception>
    public Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    /// <summary>
    /// Toggles the follow relationship from <paramref name="followerId"/> to <paramref name="followingId"/>.
    /// If already following, unfollows. If not following, follows.
    /// </summary>
    /// <param name="followerId">The ID of the user initiating the follow/unfollow.</param>
    /// <param name="followingId">The ID of the user being followed or unfollowed.</param>
    /// <returns><c>true</c> if the result is now following; <c>false</c> if unfollowed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a user attempts to follow themselves.</exception>
    public Task<bool> ToggleFollowAsync(string followerId, string followingId);

    /// <summary>
    /// Searches for users whose usernames contain the given query string (case-insensitive).
    /// Returns up to 20 results.
    /// </summary>
    /// <param name="query">The search term to match against usernames.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsFollowedByCurrentUser</c>.</param>
    /// <returns>A list of up to 20 matching <see cref="UserSearchDto"/>.</returns>
    public Task<List<UserSearchDto>> SearchUsersAsync(string query, string currentUserId);
}
