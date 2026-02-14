namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Fractal;
using FractPal.Model.DTO.Fractal;

public interface IFractalService
{
    Task<FractalFeedResponse> GetFeedAsync(string userId, int page = 1, int pageSize = 20);
    Task<List<FractalDto>> GetUserFractalsAsync(string userId, string currentUserId);
    Task<List<FractalDto>> GetPublishedFractalsByUserAsync(string userId, string currentUserId);
    Task<FractalDto?> GetFractalByIdAsync(string fractalId, string currentUserId);
    Task<FractalDto> CreateFractalAsync(string userId, CreateFractalRequest request);
    Task<FractalDto> UpdateFractalAsync(string fractalId, string userId, UpdateFractalRequest request);
    Task DeleteFractalAsync(string fractalId, string userId);
    Task<FractalDto> PublishFractalAsync(string fractalId, string userId);
    Task<FractalDto> UnpublishFractalAsync(string fractalId, string userId);
    Task<bool> ToggleLikeAsync(string fractalId, string userId);
}
