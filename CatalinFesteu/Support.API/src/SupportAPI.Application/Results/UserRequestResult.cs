using SupportAPI.Domain.Entities;
using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Application.Results
{
    public sealed record UserRequestResult(Guid RequestId,
        string Title,
        string RequestContent, 
        RequestStatus Status);
}