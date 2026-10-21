using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Policies;
using Insurance.Application.DTO.Premiums;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Services;

public sealed class PolicyService(
        IPolicyRepository policies,
        IClientRepository clients,
        IBuildingRepository buildings,
        IBrokerRepository brokers,
        ICurrencyRepository currencies,
        IPremiumCalculationService premium) : IPolicyService
{
    public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var client = await clients.GetClientByIdAsync(request.ClientId, cancellationToken)
            ?? throw new NotFoundException("Client was not found.");
        var building = await buildings.GetBuildingByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new NotFoundException("Building was not found.");
        if (building.ClientId != client.Id)
        {
            throw new ArgumentException("The building does not belong to the selected client.");
        }
        var broker = await brokers.GetBrokerByIdAsync(request.BrokerId, cancellationToken)
            ?? throw new NotFoundException("Broker was not found.");
        if (broker.Status != BrokerStatus.Active)
        {
            throw new InvalidOperationException("Inactive brokers cannot create policies.");
        }
        var currency = await currencies.GetCurrencyByIdAsync(request.CurrencyId, cancellationToken)
            ?? throw new NotFoundException("Currency was not found.");
        if (!currency.IsActive)
        {
            throw new InvalidOperationException("Inactive currencies cannot be used for new policies.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.BasePremium);

        var finalPremium = await premium.CalculateFinalPremiumAsync(
            request.BasePremium,
            request.StartDate,
            new PremiumCalculationContext
            {
                CityId = building.CityId,
                BuildingType = building.Type
            },
            cancellationToken);
        var policy = new Policy(
            $"POL-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            client.Id,
            building.Id,
            broker.Id,
            currency.Id,
            request.StartDate,
            request.EndDate,
            request.BasePremium,
            finalPremium);
        await policies.AddPolicyAsync(policy, cancellationToken);
        return MapToPolicyDto(policy);
    }

    public async Task<PolicyDto?> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var policy = await policies.GetPolicyByIdAsync(id, cancellationToken);
        return policy is null ? null : MapToPolicyDto(policy);
    }

    public async Task<PagedResult<PolicyDto>> SearchPolicyAsync(
        string? policyNumber,
        Guid? clientId,
        Guid? brokerId,
        PolicyStatus? status,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        if (startDateFrom.HasValue && startDateTo.HasValue && startDateFrom > startDateTo)
        {
            throw new ArgumentException("The start date cannot be after the end date.");
        }
        var result = await policies.SearchPoliciesAsync(
            policyNumber,
            clientId,
            brokerId,
            status,
            startDateFrom,
            startDateTo,
            pagination,
            cancellationToken);
        return new PagedResult<PolicyDto>
        {
            Items = [.. result.Items.Select(MapToPolicyDto)],
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<PolicyDto> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken)
    {
        var policy = await policies.GetPolicyByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Policy was not found.");
        policy.Activate(DateTime.UtcNow);
        await policies.UpdatePolicyAsync(policy, cancellationToken);
        return MapToPolicyDto(policy);
    }

    public async Task<PolicyDto> CancelPolicyAsync(Guid id, CancelPolicyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var policy = await policies.GetPolicyByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Policy was not found.");
        policy.Cancel(request.Reason, request.CancellationDate);
        await policies.UpdatePolicyAsync(policy, cancellationToken);
        return MapToPolicyDto(policy);
    }

    private static PolicyDto MapToPolicyDto(Policy p) => new()
    {
        Id = p.Id,
        PolicyNumber = p.PolicyNumber,
        ClientId = p.ClientId,
        BuildingId = p.BuildingId,
        BrokerId = p.BrokerId,
        CurrencyId = p.CurrencyId,
        Status = p.Status,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        BasePremium = p.BasePremium,
        FinalPremium = p.FinalPremium,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        CancellationDate = p.CancellationDate,
        CancellationReason = p.CancellationReason
    };
}
