namespace SupportAPI.Application.Commands
{
    public record CreateSupportRequestCommand(
        string Title,
        string RequestContent);
}
