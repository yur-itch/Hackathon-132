namespace PlantCare.Api.Dtos;

public record RegisterDto(string Email, string Password, string DisplayName);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(bool Authenticated);
public record UserDto(int Id, string Email, string DisplayName, DateTime CreatedAt);
public record UpdateUserDto(string DisplayName);
