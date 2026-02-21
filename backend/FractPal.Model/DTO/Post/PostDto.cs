namespace FractPal.Model.DTO.Post;

using System;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid FractalId { get; set; }
    public Guid AuthorId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = default!;
    public string? Description { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
