using FluentValidation;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.CommandHandlers
{
    public class CreateSupportRequestCommandHandler(
        ICurrentUser currentUser,
        IValidator<CreateSupportRequestCommand> validator,
        ISupportRequestsRepository requestsRepository) : ICommandHandler<CreateSupportRequestCommand, SupportRequestResult>
    {
        public async Task<SupportRequestResult> Handle(CreateSupportRequestCommand command, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);

            var request = SupportRequest.Create(currentUser.UserId, command.Title, command.RequestContent);

            await requestsRepository.AddRequestAsync(request, cancellationToken);
            await requestsRepository.SaveChangesAsync(cancellationToken);

            return new SupportRequestResult(request.Id, request.Title, request.RequestContent, request.Status);
        }
    }
}
