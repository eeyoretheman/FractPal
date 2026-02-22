namespace FractPal.Data;

using FractPal.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Fractal> Fractals { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Fractal configuration
        builder.Entity<Fractal>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasOne(f => f.User)
                  .WithMany(u => u.Fractals)
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => f.UserId);
            entity.HasIndex(f => f.IsPublished);
            entity.HasIndex(f => f.PublishedAt);
        });

        // Like configuration
        builder.Entity<Like>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Fractal)
                  .WithMany(f => f.Likes)
                  .HasForeignKey(l => l.FractalId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one like per user per fractal
            entity.HasIndex(l => new { l.UserId, l.FractalId }).IsUnique();
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

        // Comment configuration
        builder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Fractal)
                  .WithMany(f => f.Comments)
                  .HasForeignKey(c => c.FractalId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.FractalId);
            entity.HasIndex(c => c.UserId);
        });
    }
}
