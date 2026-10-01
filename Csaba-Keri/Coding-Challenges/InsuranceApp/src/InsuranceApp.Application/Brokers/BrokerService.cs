using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Mappings;
using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Domain.Brokers;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Brokers;

public class BrokerService(
    IBrokerRepository brokerRepository,
    IValidator<CreateBrokerCommand> createValidator,
    IValidator<UpdateBrokerCommand> updateValidator,
    IValidator<PageQuery> pageValidator,
    ILogger<BrokerService> logger
) : IBrokerService
{
    private readonly IBrokerRepository _brokerRepository = brokerRepository;
    private readonly IValidator<CreateBrokerCommand> _createValidator = createValidator;
    private readonly IValidator<UpdateBrokerCommand> _updateValidator = updateValidator;
    private readonly IValidator<PageQuery> _pageValidator = pageValidator;
    private readonly ILogger<BrokerService> _logger = logger;

    public async Task<BrokerResult> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var broker = await GetBrokerByIdOrThrowAsync(brokerId, cancellationToken);

        return broker.ToResult();
    }

    public async Task<PagedResult<BrokerResult>> GetBrokersAsync(PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var page = await _brokerRepository.GetBrokersAsync(query, cancellationToken);

        return page.Map(broker => broker.ToResult());
    }

    public async Task<BrokerResult> CreateBrokerAsync(CreateBrokerCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var broker = new Broker(
            id: Guid.NewGuid(),
            code: command.Code,
            name: command.Name,
            email: command.Email,
            phone: command.Phone,
            status: command.Status
        );

        await _brokerRepository.AddBrokerAsync(broker, cancellationToken);
        _logger.LogInformation("Broker {BrokerId} created.", broker.Id);

        return broker.ToResult();
    }

    public async Task<BrokerResult> UpdateBrokerAsync(UpdateBrokerCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var broker = await GetBrokerByIdOrThrowAsync(command.BrokerId, cancellationToken);

        broker.UpdateDetails(
            name: command.Name,
            email: command.Email,
            phone: command.Phone
        );

        await _brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        _logger.LogInformation("Broker {BrokerId} details updated.", broker.Id);
        
        return broker.ToResult();
    }

    public Task<BrokerResult> ActivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        return ChangeBrokerStatusAsync(brokerId, BrokerStatus.Active, cancellationToken);
    }

    public Task<BrokerResult> DeactivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        return ChangeBrokerStatusAsync(brokerId, BrokerStatus.Inactive, cancellationToken);
    }

    private async Task<Broker> GetBrokerByIdOrThrowAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        if (brokerId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("BrokerId", "Broker identifier must not be empty.");
        }

        var broker = await _brokerRepository.GetBrokerByIdAsync(brokerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Broker), brokerId);

        return broker;
    }

    private async Task<BrokerResult> ChangeBrokerStatusAsync(Guid brokerId, BrokerStatus newStatus, CancellationToken cancellationToken)
    {
        var broker = await GetBrokerByIdOrThrowAsync(brokerId, cancellationToken);
        var oldStatus = broker.Status;

        if (oldStatus == newStatus)
        {
            return broker.ToResult();
        }

        switch (newStatus)
        {
            case BrokerStatus.Active:
                broker.Activate();
                break;

            case BrokerStatus.Inactive:
                broker.Deactivate();
                break;
        }

        await _brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        _logger.LogInformation(
            "Broker {BrokerId} status changed to {NewStatus} from {OldStatus}.",
            broker.Id, broker.Status, oldStatus
        );
        
        return broker.ToResult();
    }
}
