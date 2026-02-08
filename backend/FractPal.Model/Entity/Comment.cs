namespace FractPal.Model.Entity;

using System.ComponentModel.DataAnnotations.Schema;
using FractPal.Model.Entity.Abstractions;

/// <summary>
/// Represents a comment left by a user on a post.
/// </summary>
/// <remarks>
/// Inherits common entity properties from <see cref="BaseEntity"/> and exposes
/// audit fields defined by <see cref="IAuditable"/>.
/// </remarks>
public class Comment : BaseEntity, IAuditable
{
    /// <summary>
    /// The UTC date and time when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The UTC date and time when the comment was last updated, if any.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The textual content of the comment.
    /// </summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// Foreign key referencing the <see cref="FractPalUser"/> who authored the comment.
    /// </summary>
    [ForeignKey(nameof(Author))]
    public virtual Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property for the user who authored the comment.
    /// May be <c>null</c> when the related user is not loaded.
    /// </summary>
    public virtual FractPalUser? Author { get; set; }

    /// <summary>
    /// Foreign key referencing the <see cref="Post"/> this comment belongs to.
    /// </summary>
    [ForeignKey(nameof(Post))]
    public virtual Guid PostId { get; set; }

    /// <summary>
    /// Navigation property for the post that this comment is associated with.
    /// May be <c>null</c> when the related post is not loaded.
    /// </summary>
    public virtual Post? Post { get; set; }
}
