namespace FractPal.Service.Interface;

using System;
using System.Collections.Generic;
using System.Text;

public interface ILikeService
{
    public Task<bool> ToggleLikeAsync(Guid postId, Guid userId);
}
