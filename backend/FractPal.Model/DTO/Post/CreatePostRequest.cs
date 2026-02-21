namespace FractPal.Model.DTO.Post;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

public class CreatePostRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;
}
