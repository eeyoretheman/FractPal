namespace FractPal.Data;

using System.Text.Json;
using FractPal.Model.Entities;
using FractPal.Model.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<FractPalUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Fractal> Fractals { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var dictComparer = CreateDictValueComparer();

        // Fractal configuration
        builder.Entity<Fractal>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.HasOne(f => f.Author)
                  .WithMany(u => u.Fractals)
                  .HasForeignKey(f => f.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => f.AuthorId);
            entity.HasIndex(f => f.CreatedAt);
        });

        // Post configuration
        builder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Author)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Fractal)
                  .WithMany()
                  .HasForeignKey(p => p.FractalId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.AuthorId);
            entity.HasIndex(p => p.FractalId);
            entity.HasIndex(p => p.CreatedAt);
        });

        // Comment configuration
        builder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.Author)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.AuthorId);
            entity.HasIndex(c => c.PostId);
            entity.HasIndex(c => c.CreatedAt);
        });

        // Like configuration
        builder.Entity<Like>(entity =>
        {
            // Composite key
            entity.HasKey(l => new { l.UserId, l.PostId });
            
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(l => l.PostId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Follow configuration
        builder.Entity<Follow>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.HasOne(f => f.Follower)
                  .WithMany(u => u.Following)
                  .HasForeignKey(f => f.FollowerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.FollowingUser)
                  .WithMany(u => u.Followers)
                  .HasForeignKey(f => f.FollowingId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: can't follow same user twice
            entity.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
        });
    }

    /// <summary>
    /// Creates a <see cref="ValueComparer{Dictionary}"/> suitable for comparing
    /// <see cref="Dictionary{String,List{String}}"/> instances used by EF Core value conversions.
    /// The comparer uses deep equality, a deterministic hash and a snapshot function.
    /// </summary>
    /// <returns>A configured <see cref="ValueComparer{Dictionary}"/> instance.</returns>
    private static ValueComparer<Dictionary<string, List<string>>> CreateDictValueComparer()
    {
        return new ValueComparer<Dictionary<string, List<string>>>(
            (a, b) => DictEquality(a, b),
            v => DictHash(v),
            v => DictSnapShot(v)
        );
    }

    /// <summary>
    /// Deep equality check for two dictionaries mapping string keys to ordered lists of strings.
    /// Comparison is order-sensitive for list values and uses <see cref="StringComparer.Ordinal"/> for key ordering in hashing.
    /// </summary>
    /// <param name="a">First dictionary (may be null).</param>
    /// <param name="b">Second dictionary (may be null).</param>
    /// <returns>True if both dictionaries are equal by structure and element values; otherwise false.</returns>
    private static bool DictEquality(Dictionary<string, List<string>>? a, Dictionary<string, List<string>>? b)
    {
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            if (a.Count != b.Count)
                return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bList))
                    return false;
                var aList = kvp.Value;
                if (aList is null && bList is null)
                    continue;
                if (aList is null || bList is null)
                    return false;
                if (aList.Count != bList.Count)
                    return false;
                for (int i = 0; i < aList.Count; i++)
                {
                    if (aList[i] != bList[i])
                        return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Produces a deterministic hash code for a dictionary of lists.
    /// Keys are processed in ordinal order to ensure consistent hash independent of insertion order.
    /// Each key, each list item and the list length contribute to the hash. Null dictionary returns 0.
    /// </summary>
    /// <param name="v">Dictionary to hash (may be null).</param>
    /// <returns>An integer hash code.</returns>
    private static int DictHash(Dictionary<string, List<string>>? v)
    {
        if (v is null)
            return 0;
        var hash = new HashCode();
        foreach (var key in v.Keys.OrderBy(k => k, StringComparer.Ordinal))
        {
            hash.Add(key, StringComparer.Ordinal);
            var list = v[key];
            if (list != null)
            {
                foreach (var item in list)
                {
                    hash.Add(item);
                }
                hash.Add(list.Count);
            }
            else
            {
                // Marker to distinguish null list vs empty list (empty lists add count 0 above)
                hash.Add(-1);
            }
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Creates a deep snapshot copy of the provided dictionary and its inner lists.
    /// Used by EF Core to take a snapshot of a value object so changes can be detected.
    /// </summary>
    /// <param name="v">The dictionary to snapshot.</param>
    /// <returns>A new dictionary containing copies of the inner lists.</returns>
    private static Dictionary<string, List<string>> DictSnapShot(Dictionary<string, List<string>> v)
    {
        if (v == null)
            return new Dictionary<string, List<string>>();
        var copy = new Dictionary<string, List<string>>(v.Count);
        foreach (var kvp in v)
        {
            copy[kvp.Key] = kvp.Value == null ? new List<string>() : new List<string>(kvp.Value);
        }
        return copy;
    }
}
