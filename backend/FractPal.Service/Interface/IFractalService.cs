namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Fractal;

public interface IFractalService
{
    public Task<FractalFeedResponse> GetFeedAsync(string userId, int page = 1, int pageSize = 20);
    public Task<List<FractalDto>> GetUserFractalsAsync(string userId, string currentUserId);
    public Task<List<FractalDto>> GetPublishedFractalsByUserAsync(string userId, string currentUserId);
    public Task<FractalDto?> GetFractalByIdAsync(string fractalId, string currentUserId);
    public Task<FractalDto> CreateFractalAsync(string userId, CreateFractalRequest request);
    public Task<FractalDto> UpdateFractalAsync(string fractalId, string userId, UpdateFractalRequest request);
    public Task DeleteFractalAsync(string fractalId, string userId);
    // public Task<FractalDto> PublishFractalAsync(string fractalId, string userId);
    // public Task<FractalDto> UnpublishFractalAsync(string fractalId, string userId);
    public Task<FractalDto> ForkFractalAsync(string fractalId, string userId);
    // public Task<bool> ToggleLikeAsync(string fractalId, string userId);
}
