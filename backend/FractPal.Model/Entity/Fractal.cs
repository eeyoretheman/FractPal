namespace FractPal.Model.Entities;

public class Fractal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; } = false;

    // L-System configuration stored as JSON
    public string Axiom { get; set; } = string.Empty;
    public string Rules { get; set; } = string.Empty; // JSON
    public string Instructions { get; set; } = string.Empty; // JSON
    public int Generations { get; set; }
    public double XTranslation { get; set; }
    public double YTranslation { get; set; }
    public double Zoom { get; set; } = 1.0;

    // Rendered image
    public string? ImageUrl { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Like> Likes { get; set; } = [];
    public virtual ICollection<Comment> Comments { get; set; } = [];
}
