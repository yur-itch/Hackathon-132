using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces;

namespace PlantCare.Api.Services.Implementations;

public class PlantsService : IPlantsService
{
    private readonly AppDbContext _db;

    public PlantsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<Plant>> GetPlantsAsync(bool? isPoisonous)
    {
        var q = _db.Plants.AsNoTracking();

        if (isPoisonous.HasValue)
        {
            if (isPoisonous.Value)
            {
                q = q.Where(p => p.Toxicity != null && p.Toxicity != "" && !p.Toxicity.ToLower().Contains("non-toxic") && !p.Toxicity.ToLower().Contains("not toxic"));
            }
            else
            {
                q = q.Where(p => p.Toxicity == null || p.Toxicity == "" || p.Toxicity.ToLower().Contains("non-toxic") || p.Toxicity.ToLower().Contains("not toxic"));
            }
        }

        return await q.ToListAsync();
    }

    public async Task<Plant?> GetPlantByIdAsync(int id)
    {
        return await _db.Plants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }
}
