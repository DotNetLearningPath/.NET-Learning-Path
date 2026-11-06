using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Exceptions;
using SupportAPI.Application.Queries;
using SupportAPI.Application.Results;

namespace SupportAPI.Application.QueryHandlers
{
    public class GetSupportRequestByIdQueryHandler(
        ISupportRequestsRepository requestsRepository,
        ICurrentUser currentUser,
        IValidator<GetSupportRequestByIdQuery> validator) : IQueryHandler<GetSupportRequestByIdQuery, SupportRequestResult>
    {
        public async Task<SupportRequestResult?> Handle(GetSupportRequestByIdQuery query, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(query, cancellationToken);
            var userId = currentUser.UserId;

            var request = await requestsRepository.GetRequestByIdAsync(userId, query.RequestId, cancellationToken);

            return request == null
                ? throw new EntityNotFoundException("Support request not found")
                : new SupportRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
        }
    }
}
