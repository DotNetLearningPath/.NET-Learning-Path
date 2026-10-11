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
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Policies;

public class PolicyService(
    IPolicyRepository policyRepository,
    IBuildingRepository buildingRepository,
    IBrokerRepository brokerRepository,
    ICurrencyRepository currencyRepository,
    IValidator<CreateDraftPolicyCommand> createValidator,
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
    private readonly IPolicyPremiumCalculator _premiumCalculator = premiumCalculator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<PolicyService> _logger = logger;

    public async Task<PolicyResult> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var policy = await GetPolicyByIdOrThrowAsync(policyId, cancellationToken);

        var currency = await _currencyRepository.GetCurrencyByIdAsync(policy.CurrencyId, cancellationToken)
            ?? throw new InvalidOperationException("The policy references a currency that does not exist.");

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

    private async Task<Policy> GetPolicyByIdOrThrowAsync(Guid policyId, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create(nameof(policyId), "Policy identifier must not be empty.");
        }

        return await _policyRepository.GetPolicyByIdAsync(policyId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Policy), policyId);
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

    private static void ValidateBrokerIsActive(Broker broker)
    {
        if (broker.Status != BrokerStatus.Active)
        {
            throw ValidationExceptionFactory.Create(
                nameof(CreateDraftPolicyCommand.BrokerId),
                "Inactive brokers cannot create policies."
            );
        }
    }

    private static void ValidateCurrencyIsActive(Currency currency)
    {
        if (!currency.IsActive)
        {
            throw ValidationExceptionFactory.Create(
                nameof(CreateDraftPolicyCommand.CurrencyId),
                "Inactive currencies cannot be used for new policies."
            );
        }
    }
}
