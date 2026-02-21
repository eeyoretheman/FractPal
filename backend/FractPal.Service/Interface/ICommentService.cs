namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Comment;

/// <summary>
/// Provides business logic for creating, retrieving, updating, and deleting
/// comments on FractPal posts.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Retrieves a single comment by its unique identifier.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns>The matching <see cref="CommentDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no comment exists with <paramref name="commentId"/>.</exception>
    public Task<CommentDto> GetCommentById(Guid commentId);

    /// <summary>
    /// Retrieves all comments on a given post, ordered chronologically.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>A list of <see cref="CommentDto"/> for the post.</returns>
    public Task<List<CommentDto>> GetPostComments(Guid postId);

    /// <summary>
    /// Creates a new comment on the specified post.
    /// </summary>
    /// <param name="userId">The ID of the user creating the comment.</param>
    /// <param name="postId">The ID of the post being commented on.</param>
    /// <param name="request">The comment content.</param>
    /// <returns>The newly created <see cref="CommentDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no post exists with <paramref name="postId"/>.</exception>
    public Task<CommentDto> CreateComment(Guid userId, Guid postId, CreateCommentRequest request);

    /// <summary>
    /// Updates the content of an existing comment. Only the comment's author may edit it.
    /// </summary>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="request">The updated comment content.</param>
    /// <returns>The updated <see cref="CommentDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no comment exists with <paramref name="commentId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the comment's author.</exception>
    public Task<CommentDto> UpdateComment(Guid userId, Guid commentId, UpdateCommentRequest request);

    /// <summary>
    /// Permanently deletes a comment. Only the comment's author may perform this operation.
    /// </summary>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="commentId">The ID of the comment to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no comment exists with <paramref name="commentId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the comment's author.</exception>
    public Task DeleteComment(Guid userId, Guid commentId);
}
