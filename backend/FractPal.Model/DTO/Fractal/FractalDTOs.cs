namespace FractPal.Model.DTO.Fractal;

using System.ComponentModel.DataAnnotations;

public class FractalDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public string? ImageUrl { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }

    // L-System data
    public string Axiom { get; set; } = string.Empty;
    public string Rules { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Generations { get; set; }
    public double XTranslation { get; set; }
    public double YTranslation { get; set; }
    public double Zoom { get; set; }
}

public class CreateFractalRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Axiom { get; set; } = string.Empty;

    [Required]
    public string Rules { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Required]
    public int Generations { get; set; }

    public double XTranslation { get; set; }
    public double YTranslation { get; set; }
    public double Zoom { get; set; } = 1.0;

    // Optional base64 image
    public string? ImageData { get; set; }
}

public class UpdateFractalRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Axiom { get; set; } = string.Empty;

    [Required]
    public string Rules { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Required]
    public int Generations { get; set; }

    public double XTranslation { get; set; }
    public double YTranslation { get; set; }
    public double Zoom { get; set; }

    public string? ImageData { get; set; }
}

public class FractalFeedResponse
{
    public List<FractalDto> Fractals { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
