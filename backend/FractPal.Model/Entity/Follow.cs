namespace FractPal.Model.Entities;

public class Follow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FollowerId { get; set; } = string.Empty;
    public string FollowingId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User Follower { get; set; } = null!;
    public virtual User FollowingUser { get; set; } = null!;
}
