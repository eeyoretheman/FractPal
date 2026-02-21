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
            // Composite key: (UserId, FractalId)
            entity.HasKey(l => new { l.UserId, l.FractalId });

            entity.HasOne(l => l.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Fractal)
                  .WithMany(f => f.Likes)
                  .HasForeignKey(l => l.FractalId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.FractalId);
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
}
