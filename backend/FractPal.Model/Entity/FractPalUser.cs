namespace FractPal.Model.Entity;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application user entity that extends the ASP.NET Core Identity user with a <see cref="Guid"/> key.
/// Contains additional profile fields and navigation properties for related domain entities.
/// </summary>
public class FractPalUser : IdentityUser<Guid>
{
    // Custom properties

    /// <summary>
    /// Gets or sets the user's first name.
    /// Nullable to support optional profile information.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// Nullable to support optional profile information.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the user record was created.
    /// Defaults to the current UTC time when the instance is constructed.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the file system or storage path to the user's profile picture.
    /// Non-nullable; callers should ensure a valid default or handle the empty-path case.
    /// </summary>
    public string ProfilePicturePath { get; set; } = default!;

    /// <summary>
    /// Navigation property: collection of fractals created by the user.
    /// Marked virtual for EF lazy loading (if enabled).
    /// Nullable to allow for absence of related entities.
    /// </summary>
    public virtual ICollection<Fractal>? Fractals { get; set; }

    /// <summary>
    /// Navigation property: collection of posts authored by the user.
    /// Marked virtual for EF lazy loading (if enabled).
    /// Nullable to allow for absence of related entities.
    /// </summary>
    public virtual ICollection<Post>? Posts { get; set; }

    /// <summary>
    /// Navigation property: collection of comments authored by the user.
    /// Marked virtual for EF lazy loading (if enabled).
    /// Nullable to allow for absence of related entities.
    /// </summary>
    public virtual ICollection<Comment>? Comments { get; set; }

    /// <summary>
    /// Navigation property: collection of liked posts by the user.
    /// Marked virtual for EF lazy loading (if enabled).
    /// Nullable to allow for absence of related entities.
    /// </summary>
    public virtual ICollection<Like>? Likes { get; set; }
}
