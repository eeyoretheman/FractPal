namespace FractPal.Service.Interface;

/// <summary>
/// Provides like/unlike toggle operations for fractals and posts.
/// </summary>
public interface ILikeService
{
    /// <summary>
    /// Toggles the like state for a fractal by the specified user.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns><c>true</c> if the fractal is now liked; <c>false</c> if the like was removed.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no fractal exists with <paramref name="fractalId"/>.</exception>
    public Task<bool> ToggleLikeAsync(Guid fractalId, Guid userId);

    /// <summary>
    /// Toggles the like state for a post by resolving it to its underlying fractal.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns><c>true</c> if the post's fractal is now liked; <c>false</c> if the like was removed.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no post exists with <paramref name="postId"/>.</exception>
    public Task<bool> ToggleLikePostAsync(Guid postId, Guid userId);
}
