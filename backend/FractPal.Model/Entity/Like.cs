namespace FractPal.Model.Entities;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a "like" relationship where a user likes a fractal.
/// </summary>
public class Like
{
    [ForeignKey(nameof(User))]
    public virtual Guid UserId { get; set; }

    public virtual FractPalUser? User { get; set; }

    [ForeignKey(nameof(Fractal))]
    public virtual Guid FractalId { get; set; }

    public virtual Fractal? Fractal { get; set; }
}
