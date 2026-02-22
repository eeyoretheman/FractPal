namespace FractPal.Model.Entities;

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FractalId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Fractal Fractal { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
