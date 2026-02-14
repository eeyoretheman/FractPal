namespace FractPal.Data;

using FractPal.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Fractal> Fractals { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fractal configuration
        modelBuilder.Entity<Fractal>(entity =>
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
        modelBuilder.Entity<Like>(entity =>
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
        modelBuilder.Entity<Follow>(entity =>
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
