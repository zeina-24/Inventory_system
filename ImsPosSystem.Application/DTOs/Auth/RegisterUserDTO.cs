namespace ImsPosSystem.Application.DTOs.Auth;

public class RegisterUserDTO
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
