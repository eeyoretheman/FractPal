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

    public async Task<bool> ToggleLikeAsync(Guid fractalId, Guid userId)
    {
        // Validate fractal exists
        var fractalExists = await this.context.Fractals.AnyAsync(f => f.Id == fractalId);
        if (!fractalExists)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        var existingLike = await this.context.Likes
            .FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);

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
                FractalId = fractalId,
                UserId = userId
            };

            this.context.Likes.Add(like);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Possible race — treat as already liked; alternatively rethrow/log
                return true;
            }

            return true;
        }
    }
}
