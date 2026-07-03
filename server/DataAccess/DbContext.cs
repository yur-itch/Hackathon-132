using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Models;

namespace PlantCare.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<UserPlant> UserPlants => Set<UserPlant>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ExchangeOffer> ExchangeOffers => Set<ExchangeOffer>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserPlant>().HasIndex(x => x.OwnerId);
        b.Entity<Favorite>().HasIndex(x => new { x.OwnerId, x.PlantId }).IsUnique();
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<ExchangeOffer>().HasIndex(x => x.OwnerId);
        b.Entity<ChatMessage>().HasIndex(x => x.ExchangeOfferId);

        b.Entity<Reminder>()
            .HasOne(r => r.UserPlant)
            .WithMany(up => up.Reminders)
            .HasForeignKey(r => r.UserPlantId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ExchangeOffer>()
            .HasOne(o => o.UserPlant)
            .WithMany()
            .HasForeignKey(o => o.UserPlantId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<ChatMessage>()
            .HasOne(m => m.ExchangeOffer)
            .WithMany()
            .HasForeignKey(m => m.ExchangeOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
