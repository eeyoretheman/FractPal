namespace FractPal.Service.Implementation;

using FractPal.Data;
using FractPal.Model.DTO.Fractal;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.EntityFrameworkCore;

public class FractalService : IFractalService
{
    private readonly ApplicationDbContext _context;

    public FractalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FractalFeedResponse> GetFeedAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .Where(f => f.IsPublished)
            .OrderByDescending(f => f.PublishedAt);

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
        var fractals = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fractals.Select(f => MapToDto(f, currentUserId)).ToList();
    }

    public async Task<List<FractalDto>> GetPublishedFractalsByUserAsync(string userId, string currentUserId)
    {
        var fractals = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .Where(f => f.UserId == userId && f.IsPublished)
            .OrderByDescending(f => f.PublishedAt)
            .ToListAsync();

        return fractals.Select(f => MapToDto(f, currentUserId)).ToList();
    }

    public async Task<FractalDto?> GetFractalByIdAsync(string fractalId, string currentUserId)
    {
        var fractal = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId);

        return fractal == null ? null : MapToDto(fractal, currentUserId);
    }

    public async Task<FractalDto> CreateFractalAsync(string userId, CreateFractalRequest request)
    {
        var fractal = new Fractal
        {
            Name = request.Name,
            UserId = userId,
            Axiom = request.Axiom,
            Rules = request.Rules,
            Instructions = request.Instructions,
            Generations = request.Generations,
            XTranslation = request.XTranslation,
            YTranslation = request.YTranslation,
            Zoom = request.Zoom,
            ImageUrl = request.ImageData, // Store base64 or process separately
            IsPublished = false
        };

        _context.Fractals.Add(fractal);
        await _context.SaveChangesAsync();

        await _context.Entry(fractal).Reference(f => f.User).LoadAsync();

        return MapToDto(fractal, userId);
    }

    public async Task<FractalDto> UpdateFractalAsync(string fractalId, string userId, UpdateFractalRequest request)
    {
        var fractal = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId);

        if (fractal == null)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        if (fractal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this fractal");
        }

        fractal.Name = request.Name;
        fractal.Axiom = request.Axiom;
        fractal.Rules = request.Rules;
        fractal.Instructions = request.Instructions;
        fractal.Generations = request.Generations;
        fractal.XTranslation = request.XTranslation;
        fractal.YTranslation = request.YTranslation;
        fractal.Zoom = request.Zoom;

        if (!string.IsNullOrEmpty(request.ImageData))
        {
            fractal.ImageUrl = request.ImageData;
        }

        await _context.SaveChangesAsync();

        return MapToDto(fractal, userId);
    }

    public async Task DeleteFractalAsync(string fractalId, string userId)
    {
        var fractal = await _context.Fractals.FindAsync(fractalId);

        if (fractal == null)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        if (fractal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this fractal");
        }

        _context.Fractals.Remove(fractal);
        await _context.SaveChangesAsync();
    }

    public async Task<FractalDto> PublishFractalAsync(string fractalId, string userId)
    {
        var fractal = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId);

        if (fractal == null)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        if (fractal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to publish this fractal");
        }

        fractal.IsPublished = true;
        fractal.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(fractal, userId);
    }

    public async Task<FractalDto> UnpublishFractalAsync(string fractalId, string userId)
    {
        var fractal = await _context.Fractals
            .Include(f => f.User)
            .Include(f => f.Likes)
            .FirstOrDefaultAsync(f => f.Id == fractalId);

        if (fractal == null)
        {
            throw new KeyNotFoundException("Fractal not found");
        }

        if (fractal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to unpublish this fractal");
        }

        fractal.IsPublished = false;

        await _context.SaveChangesAsync();

        return MapToDto(fractal, userId);
    }

    public async Task<bool> ToggleLikeAsync(string fractalId, string userId)
    {
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);

        if (existingLike != null)
        {
            // Unlike
            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return false;
        }
        else
        {
            // Like
            var like = new Like
            {
                FractalId = fractalId,
                UserId = userId
            };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    private FractalDto MapToDto(Fractal fractal, string currentUserId)
    {
        return new FractalDto
        {
            Id = fractal.Id,
            Name = fractal.Name,
            Username = fractal.User?.UserName ?? "",
            UserId = fractal.UserId,
            CreatedAt = fractal.CreatedAt,
            PublishedAt = fractal.PublishedAt,
            IsPublished = fractal.IsPublished,
            ImageUrl = fractal.ImageUrl,
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
}
