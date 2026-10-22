using FluentValidation;
using InsuranceApp.Application.Brokers;
using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Currencies;
using InsuranceApp.Application.Policies.Commands;
using InsuranceApp.Application.Policies.Mappings;
using InsuranceApp.Application.Policies.Pricing;
using InsuranceApp.Application.Policies.Results;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Currencies;
using InsuranceApp.Domain.Policies;
using InsuranceApp.Domain.Policies.Exceptions;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Policies;

public class PolicyService(
    IPolicyRepository policyRepository,
    IBuildingRepository buildingRepository,
    IBrokerRepository brokerRepository,
    ICurrencyRepository currencyRepository,
    IValidator<CreateDraftPolicyCommand> createValidator,
    IValidator<CancelPolicyCommand> cancelValidator,
    IPolicyPremiumCalculator premiumCalculator,
    TimeProvider timeProvider,
    ILogger<PolicyService> logger
) : IPolicyService
{
    private readonly IPolicyRepository _policyRepository = policyRepository;
    private readonly IBuildingRepository _buildingRepository = buildingRepository;
    private readonly IBrokerRepository _brokerRepository = brokerRepository;
    private readonly ICurrencyRepository _currencyRepository = currencyRepository;
    private readonly IValidator<CreateDraftPolicyCommand> _createValidator = createValidator;
    private readonly IValidator<CancelPolicyCommand> _cancelValidator = cancelValidator;
    private readonly IPolicyPremiumCalculator _premiumCalculator = premiumCalculator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<PolicyService> _logger = logger;

    public async Task<PolicyResult> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var policy = await GetPolicyByIdOrThrowAsync(policyId, cancellationToken);

        var currency = await GetPolicyCurrencyByIdOrThrowAsync(policy.CurrencyId, cancellationToken);

        return policy.ToResult(currency.Code);
    }

    public async Task<PolicyResult> CreateDraftPolicyAsync(CreateDraftPolicyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var building = await _buildingRepository.GetBuildingByIdAsync(command.BuildingId, cancellationToken)
            ?? throw ValidationExceptionFactory.Create(nameof(command.BuildingId), "Building does not exist.");

        ValidateBuildingOwnership(building, command.ClientId);

        var broker = await _brokerRepository.GetBrokerByIdAsync(command.BrokerId, cancellationToken)
            ?? throw ValidationExceptionFactory.Create(nameof(command.BrokerId), "Broker does not exist.");

        ValidateBrokerIsActive(broker);

        var currency = await _currencyRepository.GetCurrencyByIdAsync(command.CurrencyId, cancellationToken)
            ?? throw ValidationExceptionFactory.Create(nameof(command.CurrencyId), "Currency does not exist.");

        ValidateCurrencyIsActive(currency);

        var now = _timeProvider.GetUtcNow();

        var premiumCalculationResult = await _premiumCalculator.CalculatePremiumAsync(
            basePremium: command.BasePremium,
            context: new(
                CityId: building.Address.CityId,
                BuildingType: building.Type,
                CalculationDate: DateOnly.FromDateTime(now.UtcDateTime)
            ),
            cancellationToken: cancellationToken
        );

        var policy = Policy.CreateDraftPolicy(
            data: new DraftPolicyData
            {
                PolicyId = Guid.NewGuid(),
                ClientId = command.ClientId,
                BuildingId = command.BuildingId,
                BrokerId = command.BrokerId,
                CurrencyId = command.CurrencyId,
                CurrencyExchangeRateToBase = currency.ExchangeRateToBase,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                BasePremium = command.BasePremium,
                FinalPremium = premiumCalculationResult.FinalPremium,
                AppliedAdjustments = premiumCalculationResult.Adjustments
            },
            now: now
        );

        await _policyRepository.AddPolicyAsync(policy, cancellationToken);
        _logger.LogInformation(
            "Draft policy {PolicyId} created with final premium {FinalPremium} {CurrencyCode}.",
            policy.Id, policy.FinalPremium, currency.Code
        );

        return policy.ToResult(currency.Code);
    }

    public async Task<PolicyResult> ActivatePolicyAsync(Guid policyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var policy = await GetPolicyByIdOrThrowAsync(policyId, cancellationToken);
        var oldStatus = policy.Status;

        policy.Activate(_timeProvider.GetUtcNow());

        var broker = await GetPolicyBrokerByIdOrThrowAsync(policy.BrokerId, cancellationToken);
        EnsureBrokerAllowsActivation(broker);

        var currency = await GetPolicyCurrencyByIdOrThrowAsync(policy.CurrencyId, cancellationToken);
        EnsureCurrencyAllowsActivation(currency);

        await _policyRepository.UpdatePolicyLifecycleAsync(policy, oldStatus, cancellationToken);
        _logger.LogInformation("Policy {PolicyId} activated.", policy.Id);

        return policy.ToResult(currency.Code);
    }

    public async Task<PolicyResult> CancelPolicyAsync(CancelPolicyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _cancelValidator.ValidateAndThrowAsync(command, cancellationToken);

        var now = _timeProvider.GetUtcNow();
        
        if (command.EffectiveDate != DateOnly.FromDateTime(now.UtcDateTime))
        {
            throw ValidationExceptionFactory.Create(
                nameof(command.EffectiveDate),
                "Cancellation must take effect today (UTC)."
            );
        }

        var policy = await GetPolicyByIdOrThrowAsync(command.PolicyId, cancellationToken);
        var oldStatus = policy.Status;

        policy.Cancel(
            reason: command.Reason,
            effectiveDate: command.EffectiveDate,
            now: now
        );

        var currency = await GetPolicyCurrencyByIdOrThrowAsync(policy.CurrencyId, cancellationToken);

        await _policyRepository.UpdatePolicyLifecycleAsync(policy, oldStatus, cancellationToken);
        _logger.LogInformation("Policy {PolicyId} cancelled.", policy.Id);

        return policy.ToResult(currency.Code);
    }

    public async Task<ExpirePoliciesResult> ExpirePoliciesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expiredPoliciesCount = await _policyRepository.ExpirePoliciesAsync(_timeProvider.GetUtcNow(), cancellationToken);
        _logger.LogInformation("Expired {ExpiredCount} policies.", expiredPoliciesCount);

        return new(expiredPoliciesCount);
    }

    private async Task<Policy> GetPolicyByIdOrThrowAsync(Guid policyId, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create(nameof(policyId), "Policy identifier must not be empty.");
        }

        return await _policyRepository.GetPolicyByIdAsync(policyId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Policy), policyId);
    }

    private async Task<Currency> GetPolicyCurrencyByIdOrThrowAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        return await _currencyRepository.GetCurrencyByIdAsync(currencyId, cancellationToken)
            ?? throw new InvalidOperationException("The policy references a currency that does not exist.");
    }

    private async Task<Broker> GetPolicyBrokerByIdOrThrowAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        return await _brokerRepository.GetBrokerByIdAsync(brokerId, cancellationToken)
            ?? throw new InvalidOperationException("The policy references a broker that does not exist.");
    }

    private static void ValidateBuildingOwnership(Building building, Guid clientId)
    {
        if (building.ClientId != clientId)
        {
            throw ValidationExceptionFactory.Create(
                nameof(CreateDraftPolicyCommand.BuildingId),
                "Building must belong to the selected client."
            );
        }
    }

    private static bool IsBrokerActive(Broker broker)
    {
        return broker.Status == BrokerStatus.Active;
    }

    private static void ValidateBrokerIsActive(Broker broker)
    {
        if (!IsBrokerActive(broker))
        {
            throw ValidationExceptionFactory.Create(
                nameof(CreateDraftPolicyCommand.BrokerId),
                "The selected broker must be active."
            );
        }
    }

    private static void EnsureBrokerAllowsActivation(Broker broker)
    {
        if (!IsBrokerActive(broker))
        {
            throw new PolicyStateConflictException(
                "The policy cannot be activated because its broker is inactive."
            );
        }
    }

    private static void ValidateCurrencyIsActive(Currency currency)
    {
        if (!currency.IsActive)
        {
            throw ValidationExceptionFactory.Create(
                nameof(CreateDraftPolicyCommand.CurrencyId),
                "The selected currency must be active."
            );
        }
    }

    private static void EnsureCurrencyAllowsActivation(Currency currency)
    {
        if (!currency.IsActive)
        {
            throw new PolicyStateConflictException(
                "The policy cannot be activated because its currency is inactive."
            );
        }
    }
}
