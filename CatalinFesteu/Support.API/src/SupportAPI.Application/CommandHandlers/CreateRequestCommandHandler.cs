using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.CommandHandlers
{
    public class CreateRequestCommandHandler(
        ICurrentUser currentUser, 
        IValidator<CreateRequestCommand> validator,
        IRequestsRepository requestsRepository) : ICommandHandler<CreateRequestCommand, SingleRequestResult>
    {
        public async Task<SingleRequestResult> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);
            var userId = currentUser.UserId;

            var request = Request.Create(userId, command.Title, command.RequestContent);
            await requestsRepository.AddRequestAsync(request, cancellationToken);

            return new SingleRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
        }
    }
}
