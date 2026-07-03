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

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserPlant>().HasIndex(x => x.OwnerId);
        b.Entity<Favorite>().HasIndex(x => new { x.OwnerId, x.PlantId }).IsUnique();
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();

        b.Entity<Reminder>()
            .HasOne(r => r.UserPlant)
            .WithMany(up => up.Reminders)
            .HasForeignKey(r => r.UserPlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
