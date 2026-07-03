using System.ComponentModel.DataAnnotations;

namespace PlantCare.Api.Dtos;

public record RegisterDto(
    [Required(ErrorMessage = "Email обязателен."), EmailAddress(ErrorMessage = "Некорректный email.")]
    string Email,
    [Required(ErrorMessage = "Пароль обязателен."), MinLength(8, ErrorMessage = "Пароль должен быть не короче 8 символов.")]
    string Password,
    [Required(ErrorMessage = "Имя обязательно."), MinLength(2, ErrorMessage = "Имя должно быть не короче 2 символов."), MaxLength(100)]
    string DisplayName);

public record LoginDto(
    [Required(ErrorMessage = "Email обязателен.")] string Email,
    [Required(ErrorMessage = "Пароль обязателен.")] string Password);

public record UserDto(int Id, string Email, string DisplayName, DateTime CreatedAt);

public record UpdateUserDto(
    [Required(ErrorMessage = "Имя обязательно."), MinLength(2, ErrorMessage = "Имя должно быть не короче 2 символов."), MaxLength(100)]
    string DisplayName);
