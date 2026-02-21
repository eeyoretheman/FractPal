namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Post;

/// <summary>
/// Provides business logic for publishing fractals as posts, retrieving feeds,
/// and managing post lifecycle on FractPal.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Retrieves a paginated feed of all posts, ordered by most recently created.
    /// </summary>
    /// <param name="userId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The maximum number of posts to return per page.</param>
    /// <returns>A <see cref="PostFeedResponse"/> containing posts and pagination metadata.</returns>
    public Task<PostFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Retrieves a single post by its unique identifier.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <returns>The matching <see cref="PostDto"/>, or <c>null</c> if not found.</returns>
    public Task<PostDto?> GetPostByIdAsync(Guid postId, Guid currentUserId);

    /// <summary>
    /// Retrieves all posts authored by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose posts to retrieve.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <returns>A list of <see cref="PostDto"/> for the specified user.</returns>
    public Task<List<PostDto>> GetUserPostsAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// Publishes a fractal as a post. Only the fractal's owner may publish it.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal to publish.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="request">The post metadata including name and description.</param>
    /// <returns>The newly created <see cref="PostDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no fractal exists with <paramref name="fractalId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the fractal's owner.</exception>
    public Task<PostDto> PublishFractalAsync(Guid fractalId, Guid userId, CreatePostRequest request);

    /// <summary>
    /// Removes the published post associated with a fractal. Only the post's author may unpublish it.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal whose post to remove.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no matching post exists.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the post's author.</exception>
    public Task UnpublishFractalAsync(Guid fractalId, Guid userId);

    /// <summary>
    /// Updates the metadata (name and description) of an existing post. Only the post's author may update it.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to update.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="request">The updated post metadata.</param>
    /// <returns>The updated <see cref="PostDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no post exists with <paramref name="postId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the post's author.</exception>
    public Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request);

    /// <summary>
    /// Permanently deletes a post. Admins may delete any post; regular users may only delete their own.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to delete.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user has admin privileges.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no post exists with <paramref name="postId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when a non-admin user attempts to delete another user's post.</exception>
    public Task DeletePostAsync(Guid postId, Guid userId, bool isAdmin = false);
}
