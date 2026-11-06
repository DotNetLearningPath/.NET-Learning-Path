using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Application.Results
{
    public sealed record SupportRequestResult(Guid RequestId,
        string Title,
        string RequestContent,
        SupportRequestStatus Status);
}
