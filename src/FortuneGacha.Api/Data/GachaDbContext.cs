using Microsoft.EntityFrameworkCore;
using FortuneGacha.Api.Models;

namespace FortuneGacha.Api.Data;

public class GachaDbContext : DbContext
{
    public GachaDbContext(DbContextOptions<GachaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyFortune> DailyFortunes => Set<DailyFortune>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Decoration> Decorations => Set<Decoration>();
    public DbSet<UserDecoration> UserDecorations => Set<UserDecoration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Friendship relationships
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany()
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Receiver)
            .WithMany()
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // User and DailyFortune
        modelBuilder.Entity<DailyFortune>()
            .HasOne(d => d.User)
            .WithMany(u => u.DailyFortunes)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Likes
        modelBuilder.Entity<Like>()
            .HasOne(l => l.DailyFortune)
            .WithMany(d => d.Likes)
            .HasForeignKey(l => l.DailyFortuneId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserDecorations composite key
        modelBuilder.Entity<UserDecoration>()
            .HasKey(ud => new { ud.UserId, ud.DecorationId });
    }
}
