namespace FractPal.Model.Entities;

using FractPal.Model.Entities.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a saved fractal definition and its rendering metadata.
/// </summary>
/// <remarks>
/// This entity stores the L-system definition (axiom and production rules),
/// drawing instructions for each symbol, rendering transforms (translation/zoom),
/// and audit information. It also contains a reference to the authoring user.
/// </remarks>
public class Fractal : BaseEntity, IAuditable
{
    /// <summary>
    /// Human-readable name of the fractal.
    /// </summary>
    [Required]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The initial axiom (starting string) for the L-system.
    /// </summary>
    [Required]
    public string Axiom { get; set; } = default!;

    /// <summary>
    /// Production rules for the L-system.
    /// </summary>
    /// <remarks>
    /// Key: symbol (single-character or token) to be replaced.
    /// Value: list of possible replacement strings.
    /// Multiple replacements allow stochastic or alternate productions.
    /// Example:
    /// { "F": new List&lt;string&gt; { "F+F", "F-F" } }
    /// </remarks>
    [Required]
    public Dictionary<string, List<string>> Rules { get; set; } = default!;

    /// <summary>
    /// Drawing instructions associated with each symbol.
    /// </summary>
    /// <remarks>
    /// Key: symbol used in the generated string.
    /// Value: ordered list of drawing commands or parameters interpreted by the renderer.
    /// Commands are renderer-specific (e.g., "MoveForward", "TurnLeft:90").
    /// </remarks>
    [Required]
    public Dictionary<string, List<string>> Instructions { get; set; } = default!;

    /// <summary>
    /// Number of L-system generations/iterations used to expand the axiom.
    /// </summary>
    [Required]
    public int Generation { get; set; }

    /// <summary>
    /// Horizontal translation applied when rendering the fractal.
    /// </summary>
    public double XTranslation { get; set; } = 0;

    /// <summary>
    /// Vertical translation applied when rendering the fractal.
    /// </summary>
    public double YTranslation { get; set; } = 0;

    /// <summary>
    /// Zoom (scale) factor applied when rendering the fractal.
    /// </summary>
    public double Zoom { get; set; } = 1;

    /// <summary>
    /// File path or URI to a thumbnail image representing the fractal.
    /// </summary>
    public string FractalThumbnailPath { get; set; } = default!;

    /// <summary>
    /// Timestamp when the fractal entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the fractal entity was last updated, if any.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key to the authoring user.
    /// </summary>
    [ForeignKey(nameof(Author))]
    public virtual Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property to the author (user) who created the fractal.
    /// </summary>
    public virtual FractPalUser? Author { get; set; }
}
