namespace FractPal.Model.DTO.Comment;

using System;
using System.Collections.Generic;
using System.Text;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
}
