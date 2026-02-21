namespace FractPal.Model.Entities;

using FractPal.Model.Entities.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a user-created post that references a <see cref="Fractal"/>.
/// </summary>
/// <remarks>
/// Inherits common identity from <see cref="BaseEntity"/> and implements
/// <see cref="IAuditable"/> to track creation and update timestamps.
/// Navigation properties are marked <c>virtual</c> to support EF Core proxies / lazy loading.
/// </remarks>
public class Post : BaseEntity, IAuditable
{

    /// <summary>
    /// The title or short name of the post.
    /// </summary>
    [Required]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional longer description or content for the post.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The date and time when the post was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the post was last updated, if applicable.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key to the post's author (<see cref="FractPalUser"/>).
    /// </summary>
    [ForeignKey(nameof(Author))]
    public virtual Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property for the post's author.
    /// </summary>
    public virtual FractPalUser? Author { get; set; }

    /// <summary>
    /// Foreign key to the associated <see cref="Fractal"/>.
    /// </summary>
    [ForeignKey(nameof(Fractal))]
    public virtual Guid FractalId { get; set; }

    /// <summary>
    /// Navigation property for the associated fractal.
    /// </summary>
    public virtual Fractal? Fractal { get; set; }

    /// <summary>
    /// Collection of comments made on this post.
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = [];
}
