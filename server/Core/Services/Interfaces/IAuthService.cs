using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public enum RegisterResult
{
    Created,
    EmailAlreadyExists,
    InvalidInput
}

public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    Task<(RegisterResult Result, User? User)> RegisterAsync(string email, string password, string displayName);

    /// <summary>
    /// Авторизация пользователя. Возвращает JWT токен и данные пользователя.
    /// </summary>
    Task<(string? Token, User? User)> LoginAsync(string email, string password);
}
