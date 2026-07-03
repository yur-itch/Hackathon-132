using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Services;

public interface IUserPlantsService
{
    Task<IReadOnlyCollection<UserPlantDto>> GetUserPlantsAsync(string ownerId);
    Task<CreateUserPlantServiceResult> AddUserPlantAsync(string ownerId, CreateUserPlantDto dto);
    Task<UserPlantDto?> UpdateUserPlantAsync(string ownerId, int id, UpdateUserPlantDto dto);
    Task<bool> DeleteUserPlantAsync(string ownerId, int id);
}

public enum CreateUserPlantResult
{
    Created,
    PlantNotFound,
    AlreadyExists
}

public sealed record CreateUserPlantServiceResult(
    CreateUserPlantResult Result,
    UserPlantDto? UserPlant);

public sealed class UserPlantsService : IUserPlantsService
{
    private readonly AppDbContext _db;

    public UserPlantsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<UserPlantDto>> GetUserPlantsAsync(string ownerId)
    {
        return await _db.UserPlants
            .AsNoTracking()
            .Include(userPlant => userPlant.Plant)
            .Where(userPlant => userPlant.OwnerId == ownerId)
            .OrderBy(userPlant => userPlant.Plant!.Name)
            .Select(userPlant => ToDto(userPlant))
            .ToListAsync();
    }

    public async Task<CreateUserPlantServiceResult> AddUserPlantAsync(
        string ownerId,
        CreateUserPlantDto dto)
    {
        var plant = await _db.Plants.FirstOrDefaultAsync(item => item.Id == dto.PlantId);

        if (plant is null)
        {
            return new CreateUserPlantServiceResult(CreateUserPlantResult.PlantNotFound, null);
        }

        var alreadyExists = await _db.UserPlants.AnyAsync(userPlant =>
            userPlant.OwnerId == ownerId && userPlant.PlantId == dto.PlantId);

        if (alreadyExists)
        {
            return new CreateUserPlantServiceResult(CreateUserPlantResult.AlreadyExists, null);
        }

        var newUserPlant = new UserPlant
        {
            OwnerId = ownerId,
            PlantId = plant.Id,
            Plant = plant,
            Nickname = plant.Name,
            Notes = dto.Note,
        };

        _db.UserPlants.Add(newUserPlant);
        await _db.SaveChangesAsync();

        return new CreateUserPlantServiceResult(
            CreateUserPlantResult.Created,
            ToDto(newUserPlant, dto.NextWateringDate, dto.NextRepottingDate));
    }

    public async Task<UserPlantDto?> UpdateUserPlantAsync(string ownerId, int id, UpdateUserPlantDto dto)
    {
        var userPlant = await _db.UserPlants
            .Include(item => item.Plant)
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        if (userPlant is null)
        {
            return null;
        }

        userPlant.Notes = dto.Note;
        await _db.SaveChangesAsync();

        return ToDto(userPlant, dto.NextWateringDate, dto.NextRepottingDate);
    }

    public async Task<bool> DeleteUserPlantAsync(string ownerId, int id)
    {
        var userPlant = await _db.UserPlants
            .FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        if (userPlant is null)
        {
            return false;
        }

        _db.UserPlants.Remove(userPlant);
        await _db.SaveChangesAsync();
        return true;
    }

    private static UserPlantDto ToDto(UserPlant userPlant)
    {
        return ToDto(userPlant, null, null);
    }

    private static UserPlantDto ToDto(
        UserPlant userPlant,
        DateTime? nextWateringDate,
        DateTime? nextRepottingDate)
    {
        return new UserPlantDto(
            userPlant.Id,
            userPlant.PlantId ?? 0,
            userPlant.Plant?.Name ?? userPlant.Nickname,
            userPlant.Plant?.ImageUrl,
            userPlant.Notes,
            nextWateringDate,
            nextRepottingDate);
    }
}
