using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.Models.Services;

namespace InsuranceApp.Application.Services;

public sealed class PolicyReferenceService(
    IClientRepository clientRepository,
    IBuildingRepository buildingRepository,
    IBrokerRepository brokerRepository,
    ICurrencyRepository currencyRepository
    ) : IPolicyReferenceService
{
    public async Task<Result<PolicyReferences>> ValidatePolicyReferencesAsync(Guid clientId,Guid buildingId,Guid brokerId,Guid currencyId,CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.ClientNotFound);
        }

        var building = await buildingRepository.GetBuildingByIdAsync(buildingId, cancellationToken);

        if (building is null)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.BuildingNotFound);
        }

        if (building.ClientId != clientId)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.BuildingDoesNotBelongToClient);
        }

        var broker = await brokerRepository.GetBrokerByIdAsync(brokerId, cancellationToken);

        if (broker is null)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.BrokerNotFound);
        }

        if (!broker.IsActive)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.InactiveBroker);
        }

        var currency = await currencyRepository.GetCurrencyByIdAsync(currencyId, cancellationToken);

        if (currency is null)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.CurrencyNotFound);
        }

        if (!currency.IsActive)
        {
            return Result<PolicyReferences>.Failure(PolicyErrors.InactiveCurrency);
        }

        return Result<PolicyReferences>.Success(new PolicyReferences(client, building, broker, currency));
    }

}
