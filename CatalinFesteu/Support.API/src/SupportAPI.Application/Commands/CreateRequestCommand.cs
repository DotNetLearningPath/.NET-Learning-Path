using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Application.Commands
{
    public record CreateRequestCommand(
        string Title,
        string RequestContent);
}
