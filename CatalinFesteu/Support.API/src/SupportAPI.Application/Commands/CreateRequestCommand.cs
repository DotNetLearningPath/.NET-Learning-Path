using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Application.Commands
{
    public record CreateUserRequestCommand(
        string Title,
        string RequestContent);
}
