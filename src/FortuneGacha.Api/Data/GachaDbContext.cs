using Microsoft.EntityFrameworkCore;
using FortuneGacha.Api.Models;

namespace FortuneGacha.Api.Data;

public class GachaDbContext : DbContext
{
    public GachaDbContext(DbContextOptions<GachaDbContext> options) : base(options)
    {
    }

    public DbSet<GachaProfile> Users => Set<GachaProfile>();
    public DbSet<DailyFortune> DailyFortunes => Set<DailyFortune>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Decoration> Decorations { get; set; }
    public DbSet<UserDecoration> UserDecorations { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<UserQuest> UserQuests { get; set; }
    public DbSet<CoffeeFortune> CoffeeFortunes { get; set; }

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
            .HasOne(d => d.Profile)
            .WithMany(u => u.MyDailyFortunes)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Likes
        modelBuilder.Entity<Like>()
            .HasOne(l => l.DailyFortune)
            .WithMany(d => d.Likes)
            .HasForeignKey(l => l.DailyFortuneId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Profile)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserDecorations composite key
        modelBuilder.Entity<UserDecoration>()
            .HasKey(ud => new { ud.UserId, ud.DecorationId });

        modelBuilder.Entity<UserQuest>()
            .HasKey(uq => new { uq.UserId, uq.QuestId });

        // UserAchievements explicit configuration
        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.Profile)
            .WithMany()
            .HasForeignKey(ua => ua.UserId);

        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.Achievement)
            .WithMany()
            .HasForeignKey(ua => ua.AchievementId);
    }
}
