namespace FractPal.Model.DTO.Comment;

using System;
using System.Collections.Generic;
using System.Text;

public class CreateCommentRequest
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = default!;
}
