namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.Fractal;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class FractalService(ApplicationDbContext context) : IFractalService
{
    private readonly ApplicationDbContext context = context;

    public async Task<FractalFeedResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var query = this.context.Fractals
            .Include(f => f.Author)
            .Include(f => f.Likes)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var fractals = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = fractals.Select(f => MapToDto(f, userId)).ToList();

        return new FractalFeedResponse
        {
            Fractals = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<FractalDto>> GetUserFractalsAsync(Guid userId, Guid currentUserId)
    {
        var fractals = await this.context.Fractals
            .Include(f => f.Author)
            .Include(f => f.Likes)
            .Where(f => f.AuthorId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return [.. fractals.Select(f => MapToDto(f, currentUserId))];
    }

    public async Task<List<FractalDto>> GetPublishedFractalsByUserAsync(Guid userId, Guid currentUserId)
        => await this.GetUserFractalsAsync(userId, currentUserId);

    public async Task<FractalDto?> GetFractalByIdAsync(Guid fractalId, Guid currentUserId)
    {
        var fractal = await this.context.Fractals
            .Include(f => f.Author)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId);

        return fractal == null ? null : MapToDto(fractal, currentUserId);
    }

    public async Task<FractalDto> CreateFractalAsync(Guid userId, CreateFractalRequest request)
    {
        var fractal = new Fractal
        {
            Name = request.Name,
            AuthorId = userId,
            Axiom = request.Axiom,
            Rules = request.Rules,
            Instructions = request.Instructions,
            Generations = request.Generations,
            XTranslation = request.XTranslation,
            YTranslation = request.YTranslation,
            Zoom = request.Zoom,
            Thumbnail = request.Thumbnail ?? "",
            CreatedAt = DateTime.UtcNow
        };

        this.context.Fractals.Add(fractal);
        await this.context.SaveChangesAsync();

        await this.context.Entry(fractal).Reference(f => f.Author).LoadAsync();
        await this.context.Entry(fractal).Collection(f => f.Likes).LoadAsync();

        return MapToDto(fractal, userId);
    }

    public async Task<FractalDto> UpdateFractalAsync(Guid fractalId, Guid userId, UpdateFractalRequest request)
    {
        var fractal = await this.context.Fractals
            .Include(f => f.Author)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId) ?? throw new KeyNotFoundException("Fractal not found");

        // If user is not the owner, create a copy (fork) instead of updating
        if (fractal.AuthorId != userId)
        {
            var forkedFractal = new Fractal
            {
                Name = request.Name + " (Copy)",
                AuthorId = userId,
                Axiom = request.Axiom,
                Rules = request.Rules,
                Instructions = request.Instructions,
                Generations = request.Generations,
                XTranslation = request.XTranslation,
                YTranslation = request.YTranslation,
                Zoom = request.Zoom,
                Thumbnail = request.Thumbnail ?? "",
                CreatedAt = DateTime.UtcNow
            };

            this.context.Fractals.Add(forkedFractal);
            await this.context.SaveChangesAsync();
            await this.context.Entry(forkedFractal).Reference(f => f.Author).LoadAsync();
            await this.context.Entry(forkedFractal).Collection(f => f.Likes).LoadAsync();

            return MapToDto(forkedFractal, userId);
        }

        // Owner can update their own fractal
        fractal.Name = request.Name;
        fractal.Axiom = request.Axiom;
        fractal.Rules = request.Rules;
        fractal.Instructions = request.Instructions;
        fractal.Generations = request.Generations;
        fractal.XTranslation = request.XTranslation;
        fractal.YTranslation = request.YTranslation;
        fractal.Zoom = request.Zoom;
        fractal.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Thumbnail))
        {
            fractal.Thumbnail = request.Thumbnail;
        }

        await this.context.SaveChangesAsync();

        return MapToDto(fractal, userId);
    }

    public async Task DeleteFractalAsync(Guid fractalId, Guid userId)
    {
        var fractal = await this.context.Fractals.FindAsync(fractalId) ?? throw new KeyNotFoundException("Fractal not found");

        if (fractal.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this fractal");
        }

        var likes = this.context.Likes.Where(l => l.FractalId == fractalId);
        this.context.Likes.RemoveRange(likes);

        this.context.Fractals.Remove(fractal);
        await this.context.SaveChangesAsync();
    }

    public async Task<FractalDto> ForkFractalAsync(Guid fractalId, Guid userId)
    {
        var originalFractal = await this.context.Fractals
            .Include(f => f.Author)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId) ?? throw new KeyNotFoundException("Fractal not found");

        // Create a copy of the fractal for the new user
        var forkedFractal = new Fractal
        {
            Name = originalFractal.Name + " (Copy)",
            AuthorId = userId,
            Axiom = originalFractal.Axiom,
            Rules = originalFractal.Rules,
            Instructions = originalFractal.Instructions,
            Generations = originalFractal.Generations,
            XTranslation = originalFractal.XTranslation,
            YTranslation = originalFractal.YTranslation,
            Zoom = originalFractal.Zoom,
            Thumbnail = originalFractal.Thumbnail,
            CreatedAt = DateTime.UtcNow
        };

        this.context.Fractals.Add(forkedFractal);
        await this.context.SaveChangesAsync();
        await this.context.Entry(forkedFractal).Reference(f => f.Author).LoadAsync();
        await this.context.Entry(forkedFractal).Collection(f => f.Likes).LoadAsync();

        return MapToDto(forkedFractal, userId);
    }

    private static FractalDto MapToDto(Fractal fractal, Guid currentUserId) => new()
    {
        Id = fractal.Id.ToString(),
        Name = fractal.Name,
        Username = fractal.Author?.UserName ?? "",
        UserId = fractal.AuthorId.ToString(),
        CreatedAt = fractal.CreatedAt,
        PublishedAt = null, // No longer supported
        IsPublished = true, // All fractals are considered "published" now
        Thumbnail = fractal.Thumbnail,
        LikeCount = fractal.Likes?.Count ?? 0,
        IsLikedByCurrentUser = fractal.Likes?.Any(l => l.UserId == currentUserId) ?? false,
        Axiom = fractal.Axiom,
        Rules = fractal.Rules,
        Instructions = fractal.Instructions,
        Generations = fractal.Generations,
        XTranslation = fractal.XTranslation,
        YTranslation = fractal.YTranslation,
        Zoom = fractal.Zoom
    };
}
