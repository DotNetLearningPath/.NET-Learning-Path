using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Queries;
using SupportAPI.Application.Results;

namespace SupportAPI.Application.QueryHandlers
{
    public class GetUserRequestByIdQueryHandler(
        IUserRequestsRepository requestsRepository,
        ICurrentUser currentUser,
        IValidator<GetUserRequestByIdQuery> validator) : IQueryHandler<GetUserRequestByIdQuery, UserRequestResult>
    {
        public async Task<UserRequestResult?> Handle(GetUserRequestByIdQuery query, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(query, cancellationToken);

            var request = await requestsRepository.GetRequestByIdAsync(currentUser.UserId, query.RequestId, cancellationToken);

            if (request != null)
            {
                return new UserRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
            }

            return null;
        }
    }
}
