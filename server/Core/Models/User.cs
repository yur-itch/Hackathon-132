namespace PlantCare.Api.Models;

/// <summary>
/// Пользователь. Нужен только для усложнения «авторизация / синхронизация / обмен».
/// В базовой версии не используется (OwnerId = "local").
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
