namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Fractal;

public interface IFractalService
{
    public Task<FractalFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20);
    public Task<List<FractalDto>> GetUserFractalsAsync(Guid userId, Guid currentUserId);
    public Task<List<FractalDto>> GetPublishedFractalsByUserAsync(Guid userId, Guid currentUserId);
    public Task<FractalDto?> GetFractalByIdAsync(Guid fractalId, Guid currentUserId);
    public Task<FractalDto> CreateFractalAsync(Guid userId, CreateFractalRequest request);
    public Task<FractalDto> UpdateFractalAsync(Guid fractalId, Guid userId, UpdateFractalRequest request);
    public Task DeleteFractalAsync(Guid fractalId, Guid userId);
    // public Task<FractalDto> PublishFractalAsync(string fractalId, string userId);
    // public Task<FractalDto> UnpublishFractalAsync(string fractalId, string userId);
    public Task<FractalDto> ForkFractalAsync(Guid fractalId, Guid userId);
    // public Task<bool> ToggleLikeAsync(string fractalId, string userId);
}
