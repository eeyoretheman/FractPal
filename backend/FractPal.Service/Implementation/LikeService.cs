namespace FractPal.Service.Implementation;

using System;
using System.Collections.Generic;
using System.Text;
using FractPal.Data;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class LikeService(ApplicationDbContext dbContext) : ILikeService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<bool> ToggleLikeAsync(Guid postId, Guid userId)
    {
        var existingLike = await this.context.Likes
            .Include(l => l.User)
            .Include(l => l.Post)
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            this.context.Likes.Remove(existingLike);
            await this.context.SaveChangesAsync();
            return false;
        }
        else
        {
            var like = new Like
            {
                PostId = postId,
                UserId = userId
            };
            this.context.Likes.Add(like);
            await this.context.SaveChangesAsync();
            return true;
        }
    }
}
