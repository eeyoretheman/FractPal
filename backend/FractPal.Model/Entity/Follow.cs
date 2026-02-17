namespace FractPal.Model.Entities;

public class Follow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual FractPalUser Follower { get; set; } = null!;
    public virtual FractPalUser FollowingUser { get; set; } = null!;
}
