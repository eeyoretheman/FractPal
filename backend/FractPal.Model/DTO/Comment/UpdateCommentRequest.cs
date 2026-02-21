namespace FractPal.Model.DTO.Comment;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

public class UpdateCommentRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(250)]
    public string Content { get; set; } = string.Empty;
}
