namespace InsuranceApp.Application.Authentication.Commands;

public class LoginCommand(string email, string password)
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}
