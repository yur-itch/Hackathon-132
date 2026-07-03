using PlantCare.Api.Dtos;
using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя. Возвращает созданного пользователя или null в случае ошибки.
    /// </summary>
    Task<User?> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Авторизация пользователя. Возвращает JWT токен в случае успеха или null в случае неверных учетных данных.
    /// </summary>
    Task<string?> LoginAsync(LoginDto dto);
}
