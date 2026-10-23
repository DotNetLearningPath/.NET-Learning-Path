namespace SupportAPI.Application.Commands
{
    public sealed record RegisterUserCommand(string Name, string Email, string Password);
}