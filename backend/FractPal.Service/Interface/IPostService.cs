namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Post;

public interface IPostService
{
    public Task<PostFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20);
    public Task<PostDto?> GetPostByIdAsync(Guid postId, Guid currentUserId);
    public Task<List<PostDto>> GetUserPostsAsync(Guid userId, Guid currentUserId);
    public Task<PostDto> PublishFractalAsync(Guid fractalId, Guid userId, CreatePostRequest request);
    public Task UnpublishFractalAsync(Guid fractalId, Guid userId);
    public Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request);
    public Task DeletePostAsync(Guid postId, Guid userId, bool isAdmin = false);
}
