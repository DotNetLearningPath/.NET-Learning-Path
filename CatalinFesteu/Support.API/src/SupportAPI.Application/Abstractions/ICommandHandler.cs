namespace SupportAPI.Application.Abstractions
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
}