namespace FractPal.Model.Entities;

using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Fractal> Fractals { get; set; } = new List<Fractal>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
}
