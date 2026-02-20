namespace FractPal.Model.DTO.Post;

using System;
using System.Collections.Generic;
using System.Text;

public class PostFeedResponse
{
    public List<PostDto> Posts { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
}
