using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.CommandHandlers
{
    public class CreateRequestCommandHandler(
        ICurrentUser currentUser, 
        IValidator<CreateUserRequestCommand> validator,
        IRequestsRepository requestsRepository) : ICommandHandler<CreateUserRequestCommand, UserRequestResult>
    {
        public async Task<UserRequestResult> Handle(CreateUserRequestCommand command, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);

            var request = Request.Create(currentUser.UserId, command.Title, command.RequestContent);
            
            await requestsRepository.AddRequestAsync(request, cancellationToken);

            return new UserRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
        }
    }
}
