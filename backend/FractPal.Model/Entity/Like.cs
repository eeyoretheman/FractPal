namespace FractPal.Model.Entity;

using System;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a "like" relationship where a user likes a post.
/// </summary>
/// <remarks>
/// This entity models the association between <see cref="FractPalUser"/> and <see cref="Post"/>.
/// Each instance indicates that the user identified by <see cref="UserId"/> liked the post
/// identified by <see cref="PostId"/>. Navigation properties may be null if not loaded.
/// </remarks>
public class Like
{
    /// <summary>
    /// The identifier of the user who performed the like.
    /// </summary>
    /// <remarks>
    /// Foreign key that references the <see cref="FractPalUser"/> entity.
    /// </remarks>
    [ForeignKey(nameof(User))]
    public virtual Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who performed the like.
    /// </summary>
    /// <remarks>
    /// May be null when the related user entity is not loaded from the database.
    /// </remarks>
    public virtual FractPalUser? User { get; set; }

    /// <summary>
    /// The identifier of the post that was liked.
    /// </summary>
    /// <remarks>
    /// Foreign key that references the <see cref="Post"/> entity.
    /// </remarks>
    [ForeignKey(nameof(Post))]
    public virtual Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the post that was liked.
    /// </summary>
    /// <remarks>
    /// May be null when the related post entity is not loaded from the database.
    /// </remarks>
    public virtual Post? Post { get; set; }
}
