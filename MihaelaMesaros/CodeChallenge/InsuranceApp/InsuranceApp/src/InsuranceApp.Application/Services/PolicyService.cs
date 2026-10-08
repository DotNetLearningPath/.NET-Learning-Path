using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class PolicyService(
    IPolicyRepository policyRepository,
    IClientRepository clientRepository,
    IBuildingRepository buildingRepository,
    IBrokerRepository brokerRepository,
    ICurrencyRepository currencyRepository,
    IPolicyNumberGenerator policyNumberGenerator,
    ILogger<PolicyService> logger
    ) : IPolicyService
{
    public async Task<Result<PagedResult<PolicyDto>>> SearchPoliciesAsync(PolicySearchDto policySearchDto, CancellationToken cancellationToken)
    {
        if (policySearchDto.ClientId == Guid.Empty)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidClientId);
        }

        if (policySearchDto.BrokerId == Guid.Empty)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidBrokerId);
        }

        if (policySearchDto.PageNumber is < CommonConstraints.MinPageNumber or > CommonConstraints.MaxPageNumber)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidPageNumber);
        }

        if (policySearchDto.PageSize is < CommonConstraints.MinPageSize or > CommonConstraints.MaxPageSize)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidPageSize);
        }

        if (policySearchDto.Status.HasValue && !Enum.IsDefined(policySearchDto.Status.Value))
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidStatus);
        }

        if (policySearchDto.StartDate.HasValue 
            && policySearchDto.EndDate.HasValue 
            && policySearchDto.StartDate.Value > policySearchDto.EndDate.Value)
        {
            return Result<PagedResult<PolicyDto>>.Failure(PolicyErrors.InvalidDateRange);
        }

        var (policies, totalCount) = await policyRepository.SearchPoliciesAsync(policySearchDto, cancellationToken);

        var items = policies.Select(MapPolicyToDto).ToList();

        var pagedResult = new PagedResult<PolicyDto>(
            items,
            policySearchDto.PageNumber,
            policySearchDto.PageSize,
            totalCount);

        return Result<PagedResult<PolicyDto>>.Success(pagedResult);
    }

    public async Task<Result<PolicyDto>> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InvalidPolicyId);
        }

        var policy = await policyRepository.GetPolicyByIdAsync(policyId, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.NotFound(policyId));
        }

        return Result<PolicyDto>.Success(MapPolicyToDto(policy));
    }

    public async Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto createPolicyDto, CancellationToken cancellationToken)
    {
        var validationPolicyDetails = ValidatePolicyDetails(createPolicyDto);

        if (validationPolicyDetails is not null)
        {
            return Result<PolicyDto>.Failure(validationPolicyDetails);
        }

        var client = await clientRepository.GetClientByIdAsync(createPolicyDto.ClientId, cancellationToken);

        if (client is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.ClientNotFound);
        }

        var building = await buildingRepository.GetBuildingByIdAsync(createPolicyDto.BuildingId, cancellationToken);

        if (building is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.BuildingNotFound);
        }

        if (building.ClientId != createPolicyDto.ClientId)
        {
            return Result<PolicyDto>.Failure(
                PolicyErrors.BuildingDoesNotBelongToClient);
        }

        var broker = await brokerRepository.GetBrokerByIdAsync(createPolicyDto.BrokerId, cancellationToken);

        if (broker is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.BrokerNotFound);
        }

        if (!broker.IsActive)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InactiveBroker);
        }

        var currency = await currencyRepository.GetCurrencyByIdAsync(createPolicyDto.CurrencyId, cancellationToken);

        if (currency is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.CurrencyNotFound);
        }

        if (!currency.IsActive)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InactiveCurrency);
        }

        var generatedPolicyNumber = policyNumberGenerator.GeneratePolicyNumber();

        var policyNumberExists = await policyRepository.PolicyNumberExistsAsync(generatedPolicyNumber, cancellationToken);

        if (policyNumberExists)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyNumberNotGenerated);
        }
        

        var policy = new Policy
        {
            PolicyNumber = generatedPolicyNumber,
            ClientId = createPolicyDto.ClientId,
            BuildingId = createPolicyDto.BuildingId,
            BrokerId = createPolicyDto.BrokerId,
            CurrencyId = createPolicyDto.CurrencyId,
            Status = PolicyStatus.Draft,
            StartDate = createPolicyDto.StartDate,
            EndDate = createPolicyDto.EndDate,
            BasePremium = createPolicyDto.BasePremium,
            FinalPremium = createPolicyDto.BasePremium,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await policyRepository.AddPolicyAsync(policy, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyNumberNotGenerated);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Draft policy {PolicyId} has been created by broker {BrokerId}.", policy.PolicyId, policy.BrokerId);
        }

        return Result<PolicyDto>.Success(MapPolicyToDto(policy));
    }

    private static PolicyDto MapPolicyToDto(Policy policy)
    {
        return new PolicyDto(
            policy.PolicyId,
            policy.PolicyNumber,
            policy.ClientId,
            policy.BuildingId,
            policy.BrokerId,
            policy.CurrencyId,
            policy.Status,
            policy.StartDate,
            policy.EndDate,
            policy.BasePremium,
            policy.FinalPremium);
    }


    private static Error? ValidatePolicyDetails(CreatePolicyDto dto)
    {
        if (dto.ClientId == Guid.Empty)
        {
            return PolicyErrors.InvalidClientId;
        }

        if (dto.BuildingId == Guid.Empty)
        {
            return PolicyErrors.InvalidBuildingId;
        }

        if (dto.BrokerId == Guid.Empty)
        {
            return PolicyErrors.InvalidBrokerId;
        }

        if (dto.CurrencyId == Guid.Empty)
        {
            return PolicyErrors.InvalidCurrencyId;
        }

        if (!DecimalValidation.HasValidScale(dto.BasePremium, PolicyConstraints.PremiumScale))
        {
            return PolicyErrors.InvalidBasePremiumScale;
        }

        if (dto.BasePremium < PolicyConstraints.MinBasePremium)
        {
            return PolicyErrors.InvalidBasePremium;
        }

        if (dto.StartDate > dto.EndDate)
        {
            return PolicyErrors.InvalidDateRange;
        }

        return null;
    }

}
