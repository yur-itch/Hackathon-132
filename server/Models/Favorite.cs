using System.Text.Json.Serialization;

namespace PlantCare.Api.Models;

/// <summary>
/// Растение из справочника, отмеченное пользователем как избранное.
/// </summary>
public class Favorite
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = "local";

    public int PlantId { get; set; }
    [JsonIgnore]
    public Plant? Plant { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
