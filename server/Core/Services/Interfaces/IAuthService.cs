using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    Task<User?> RegisterAsync(string email, string password, string displayName);

    /// <summary>
    /// Авторизация пользователя. Возвращает JWT токен.
    /// </summary>
    Task<string?> LoginAsync(string email, string password);
}
