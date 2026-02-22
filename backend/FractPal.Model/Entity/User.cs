namespace FractPal.Model.Entities;

using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Fractal> Fractals { get; set; } = [];
    public virtual ICollection<Like> Likes { get; set; } = [];
    public virtual ICollection<Follow> Followers { get; set; } = [];
    public virtual ICollection<Follow> Following { get; set; } = [];
    public virtual ICollection<Comment> Comments { get; set; } = [];
}
