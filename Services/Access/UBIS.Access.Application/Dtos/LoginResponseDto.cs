namespace UBIS.Access.Application.Dtos;

public class LoginRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
}