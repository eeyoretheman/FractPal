namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.Fractal;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class FractalService(ApplicationDbContext context) : IFractalService
{
    private readonly ApplicationDbContext context = context;

    public async Task<FractalFeedResponse> GetFeedAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = this.context.Fractals
            .Include(f => f.Author)
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

    public async Task<List<FractalDto>> GetUserFractalsAsync(string userId, string currentUserId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        var fractals = await this.context.Fractals
            .Include(f => f.Author)
            .Where(f => f.AuthorId == userGuid)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return [.. fractals.Select(f => MapToDto(f, currentUserId))];
    }

    public async Task<List<FractalDto>> GetPublishedFractalsByUserAsync(string userId, string currentUserId)
    {
        return await GetUserFractalsAsync(userId, currentUserId);
    }

    public async Task<FractalDto?> GetFractalByIdAsync(string fractalId, string currentUserId)
    {
        if (!Guid.TryParse(fractalId, out var fractalGuid))
        {
            throw new ArgumentException("Invalid fractal ID format");
        }

        var fractal = await this.context.Fractals
            .Include(f => f.Author)
            .FirstOrDefaultAsync(f => f.Id == fractalGuid);

        return fractal == null ? null : MapToDto(fractal, currentUserId);
    }

    public async Task<FractalDto> CreateFractalAsync(string userId, CreateFractalRequest request)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID format");
        }

        var fractal = new Fractal
        {
            Name = request.Name,
            AuthorId = userGuid,
            Axiom = request.Axiom,
            Rules = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Rules) ?? new Dictionary<string, List<string>>(),
            Instructions = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Instructions) ?? new Dictionary<string, List<string>>(),
            Generation = request.Generations,
            XTranslation = request.XTranslation,
            YTranslation = request.YTranslation,
            Zoom = request.Zoom,
            FractalThumbnailPath = request.ImageData ?? "",
            CreatedAt = DateTime.UtcNow
        };

        this.context.Fractals.Add(fractal);
        await this.context.SaveChangesAsync();

        await this.context.Entry(fractal).Reference(f => f.Author).LoadAsync();

        return MapToDto(fractal, userId);
    }

    public async Task<FractalDto> UpdateFractalAsync(string fractalId, string userId, UpdateFractalRequest request)
    {
        if (!Guid.TryParse(fractalId, out var fractalGuid) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid ID format");
        }

        var fractal = await this.context.Fractals
            .Include(f => f.Author)
            .FirstOrDefaultAsync(f => f.Id == fractalGuid) ?? throw new KeyNotFoundException("Fractal not found");

        // If user is not the owner, create a copy (fork) instead of updating
        if (fractal.AuthorId != userGuid)
        {
            var forkedFractal = new Fractal
            {
                Name = request.Name + " (Copy)",
                AuthorId = userGuid,
                Axiom = request.Axiom,
                Rules = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Rules) ?? new Dictionary<string, List<string>>(),
                Instructions = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Instructions) ?? new Dictionary<string, List<string>>(),
                Generation = request.Generations,
                XTranslation = request.XTranslation,
                YTranslation = request.YTranslation,
                Zoom = request.Zoom,
                FractalThumbnailPath = request.ImageData ?? "",
                CreatedAt = DateTime.UtcNow
            };

            this.context.Fractals.Add(forkedFractal);
            await this.context.SaveChangesAsync();
            await this.context.Entry(forkedFractal).Reference(f => f.Author).LoadAsync();

            return MapToDto(forkedFractal, userId);
        }

        // Owner can update their own fractal
        fractal.Name = request.Name;
        fractal.Axiom = request.Axiom;
        fractal.Rules = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Rules) ?? fractal.Rules;
        fractal.Instructions = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(request.Instructions) ?? fractal.Instructions;
        fractal.Generation = request.Generations;
        fractal.XTranslation = request.XTranslation;
        fractal.YTranslation = request.YTranslation;
        fractal.Zoom = request.Zoom;
        fractal.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.ImageData))
        {
            fractal.FractalThumbnailPath = request.ImageData;
        }

        await this.context.SaveChangesAsync();

        return MapToDto(fractal, userId);
    }

    public async Task DeleteFractalAsync(string fractalId, string userId)
    {
        if (!Guid.TryParse(fractalId, out var fractalGuid) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid ID format");
        }

        var fractal = await this.context.Fractals.FindAsync(fractalGuid) ?? throw new KeyNotFoundException("Fractal not found");

        if (fractal.AuthorId != userGuid)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this fractal");
        }

        this.context.Fractals.Remove(fractal);
        await this.context.SaveChangesAsync();
    }

    public async Task<FractalDto> ForkFractalAsync(string fractalId, string userId)
    {
        if (!Guid.TryParse(fractalId, out var fractalGuid) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid ID format");
        }

        var originalFractal = await this.context.Fractals
            .Include(f => f.Author)
            .FirstOrDefaultAsync(f => f.Id == fractalGuid) ?? throw new KeyNotFoundException("Fractal not found");

        // Create a copy of the fractal for the new user
        var forkedFractal = new Fractal
        {
            Name = originalFractal.Name + " (Copy)",
            AuthorId = userGuid,
            Axiom = originalFractal.Axiom,
            Rules = originalFractal.Rules,
            Instructions = originalFractal.Instructions,
            Generation = originalFractal.Generation,
            XTranslation = originalFractal.XTranslation,
            YTranslation = originalFractal.YTranslation,
            Zoom = originalFractal.Zoom,
            FractalThumbnailPath = originalFractal.FractalThumbnailPath,
            CreatedAt = DateTime.UtcNow
        };

        this.context.Fractals.Add(forkedFractal);
        await this.context.SaveChangesAsync();
        await this.context.Entry(forkedFractal).Reference(f => f.Author).LoadAsync();

        return MapToDto(forkedFractal, userId);
    }

    private static FractalDto MapToDto(Fractal fractal, string currentUserId) => new()
    {
        Id = fractal.Id.ToString(),
        Name = fractal.Name,
        Username = fractal.Author?.UserName ?? "",
        UserId = fractal.AuthorId.ToString(),
        CreatedAt = fractal.CreatedAt,
        PublishedAt = null, // No longer supported
        IsPublished = true, // All fractals are considered "published" now
        ImageUrl = fractal.FractalThumbnailPath,
        LikeCount = 0, // Likes moved to Posts
        IsLikedByCurrentUser = false, // Likes moved to Posts
        Axiom = fractal.Axiom,
        Rules = JsonSerializer.Serialize(fractal.Rules),
        Instructions = JsonSerializer.Serialize(fractal.Instructions),
        Generations = fractal.Generation,
        XTranslation = fractal.XTranslation,
        YTranslation = fractal.YTranslation,
        Zoom = fractal.Zoom
    };
}
