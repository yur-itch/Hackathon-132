using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class FavoritesService : IFavoritesService
{
    private readonly AppDbContext _db;

    public FavoritesService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Favorite>> GetFavoritesAsync(string ownerId)
    {
        return await _db.Favorites
            .Include(f => f.Plant)
            .Where(f => f.OwnerId == ownerId)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync();
    }

    public async Task<bool> AddToFavoritesAsync(string ownerId, int plantId)
    {
        // 1. Проверяем существование растения в каталоге
        var plantExists = await _db.Plants.AnyAsync(p => p.Id == plantId);
        if (!plantExists) return false;

        // 2. Проверяем, нет ли уже в избранном
        var alreadyFavorite = await _db.Favorites.AnyAsync(f => f.OwnerId == ownerId && f.PlantId == plantId);
        if (alreadyFavorite) return false;

        var favorite = new Favorite
        {
            OwnerId = ownerId,
            PlantId = plantId,
            AddedAt = DateTime.UtcNow
        };

        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromFavoritesAsync(string ownerId, int plantId)
    {
        var favorite = await _db.Favorites.FirstOrDefaultAsync(f => f.OwnerId == ownerId && f.PlantId == plantId);
        if (favorite == null) return false;

        _db.Favorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFavoriteAsync(string ownerId, int plantId)
    {
        return await _db.Favorites.AnyAsync(f => f.OwnerId == ownerId && f.PlantId == plantId);
    }
}
