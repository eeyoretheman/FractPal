namespace FractPal.Model.DTO.Post;

using System;
using System.Collections.Generic;
using System.Text;

public class UpdatePostRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

}
