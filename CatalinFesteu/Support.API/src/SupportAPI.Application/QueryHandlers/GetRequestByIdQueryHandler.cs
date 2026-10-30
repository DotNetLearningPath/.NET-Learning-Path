using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Queries;
using SupportAPI.Application.Results;

namespace SupportAPI.Application.QueryHandlers
{
    public class GetRequestByIdQueryHandler(
        IRequestsRepository requestsRepository,
        ICurrentUser currentUser,
        IValidator<GetRequestByIdQuery> validator) : IQueryHandler<GetRequestByIdQuery, UserRequestResult>
    {
        public async Task<UserRequestResult?> Handle(GetRequestByIdQuery query, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(query, cancellationToken);
            var userId = currentUser.UserId;

            var request = await requestsRepository.GetRequestByIdAsync(userId, query.RequestId, cancellationToken);

            if (request != null)
            {
                return new UserRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
            }

            return null;
        }
    }
}
