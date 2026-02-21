namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Fractal;

/// <summary>
/// Provides business logic for fractal creation, retrieval, modification, and social features.
/// </summary>
public interface IFractalService
{
    /// <summary>
    /// Retrieves a paginated feed of all published fractals ordered by most recently published.
    /// </summary>
    /// <param name="userId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The maximum number of fractals to return per page.</param>
    /// <returns>A <see cref="FractalFeedResponse"/> containing fractals and pagination metadata.</returns>
    public Task<FractalFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Retrieves all fractals (published and draft) belonging to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose fractals to retrieve.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <returns>A list of <see cref="FractalDto"/> for the specified user.</returns>
    public Task<List<FractalDto>> GetUserFractalsAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// Retrieves only the published fractals belonging to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose published fractals to retrieve.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <returns>A list of published <see cref="FractalDto"/> for the specified user.</returns>
    public Task<List<FractalDto>> GetPublishedFractalsByUserAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// Retrieves a single fractal by its unique identifier.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal.</param>
    /// <param name="currentUserId">The ID of the requesting user, used to compute <c>IsLikedByCurrentUser</c>.</param>
    /// <returns>The matching <see cref="FractalDto"/>, or <c>null</c> if not found.</returns>
    public Task<FractalDto?> GetFractalByIdAsync(Guid fractalId, Guid currentUserId);

    /// <summary>
    /// Creates a new fractal as a draft owned by the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user creating the fractal.</param>
    /// <param name="request">The fractal configuration data.</param>
    /// <returns>The newly created <see cref="FractalDto"/>.</returns>
    public Task<FractalDto> CreateFractalAsync(Guid userId, CreateFractalRequest request);

    /// <summary>
    /// Updates an existing fractal. If the requesting user is not the owner, a forked copy
    /// is created instead and the original remains unchanged.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal to update.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="request">The updated fractal configuration data.</param>
    /// <returns>
    /// The updated <see cref="FractalDto"/> if the user is the owner,
    /// or a new forked <see cref="FractalDto"/> otherwise.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no fractal exists with <paramref name="fractalId"/>.</exception>
    public Task<FractalDto> UpdateFractalAsync(Guid fractalId, Guid userId, UpdateFractalRequest request);

    /// <summary>
    /// Permanently deletes a fractal. Only the owner may perform this operation.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal to delete.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no fractal exists with <paramref name="fractalId"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the requesting user is not the owner.</exception>
    public Task DeleteFractalAsync(Guid fractalId, Guid userId, bool isAdmin = false);

    /// <summary>
    /// Creates a copy of an existing fractal under a new owner. The forked fractal
    /// starts as a draft regardless of the original's publish state.
    /// </summary>
    /// <param name="fractalId">The unique identifier of the fractal to fork.</param>
    /// <param name="userId">The ID of the user who will own the fork.</param>
    /// <returns>The newly created forked <see cref="FractalDto"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no fractal exists with <paramref name="fractalId"/>.</exception>
    public Task<FractalDto> ForkFractalAsync(Guid fractalId, Guid userId);
}
