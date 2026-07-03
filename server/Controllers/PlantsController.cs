using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;

namespace PlantCare.Api.Controllers;

/// <summary>Справочник растений (общая база, только чтение для клиента).</summary>
[ApiController]
[Route("api/[controller]")]
public class PlantsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlantsController(AppDbContext db) => _db = db;

    /// <summary>Список растений с поиском и фильтром по сложности.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plant>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? difficulty)
    {
        var q = _db.Plants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search) ||
                             (p.LatinName != null && p.LatinName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(difficulty))
            q = q.Where(p => p.Difficulty == difficulty);

        return await q.OrderBy(p => p.Name).ToListAsync();
    }

    /// <summary>Карточка растения по id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Plant>> GetById(int id)
    {
        var plant = await _db.Plants.FindAsync(id);
        return plant is null ? NotFound() : plant;
    }
}
