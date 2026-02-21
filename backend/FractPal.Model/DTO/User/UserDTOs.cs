namespace FractPal.Model.DTO.User;

using System.ComponentModel.DataAnnotations;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int FractalCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}

public class UpdateProfileRequest
{
    [MaxLength(500)]
    public string? Bio { get; set; }
}

public class UserSearchDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public int FollowerCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}
