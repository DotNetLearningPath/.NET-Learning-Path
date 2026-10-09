using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Broker;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class PolicyService(
    IPolicyRepository policyRepository,
    IPolicyReferenceService policyReferenceService,
    IPolicyNumberGenerator policyNumberGenerator,
    IPolicyCalculationService policyCalculationService,
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
        var validationPolicyInputDetails = ValidatePolicyInputDetails(createPolicyDto);

        if (validationPolicyInputDetails is not null)
        {
            return Result<PolicyDto>.Failure(validationPolicyInputDetails);
        }


        var validationPolicyReferences = await policyReferenceService.ValidatePolicyReferencesAsync(
            createPolicyDto.ClientId,
            createPolicyDto.BuildingId,
            createPolicyDto.BrokerId,
            createPolicyDto.CurrencyId,
            cancellationToken
        );

        if (!validationPolicyReferences.IsSuccess)
        {
            if (validationPolicyReferences.Error is null)
            {
                return Result<PolicyDto>.Failure(PolicyErrors.PolicyReferencesValidationFailed);
            }
            else
            {
                return Result<PolicyDto>.Failure(validationPolicyReferences.Error);
            }
        }

        if (validationPolicyReferences.Value is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyReferencesValidationFailed);
        }

        var building = validationPolicyReferences.Value.Building;


        var generatedPolicyNumber = policyNumberGenerator.GeneratePolicyNumber();

        var policyNumberExists = await policyRepository.PolicyNumberExistsAsync(generatedPolicyNumber, cancellationToken);

        if (policyNumberExists)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyNumberNotGenerated);
        }

        var resultCalculateFinalPremium = 
            await policyCalculationService.CalculateFinalPremiumAsync(
                createPolicyDto.BasePremium, 
                building, 
                createPolicyDto.StartDate,
                cancellationToken);

        if (!resultCalculateFinalPremium.IsSuccess && resultCalculateFinalPremium.Error is not null)
        {
            return Result<PolicyDto>.Failure(resultCalculateFinalPremium.Error);
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
            FinalPremium = resultCalculateFinalPremium.Value,
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

    public async Task<Result<PolicyDto>> ActivatePolicyAsync(Guid policyId, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InvalidPolicyId);
        }

        var policy = await policyRepository.GetPolicyForUpdateAsync(policyId, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.NotFound(policyId));
        }

        if (policy.Status != PolicyStatus.Draft)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyNotDraft);
        }

        var datetimeNow = DateTime.UtcNow;

        if (policy.StartDate.Date < datetimeNow.Date)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.StartDateInPast);
        }


        var policyDto = new CreatePolicyDto(
            policy.ClientId,
            policy.BuildingId,
            policy.BrokerId,
            policy.CurrencyId,
            policy.StartDate,
            policy.EndDate,
            policy.BasePremium);

        var validationPolicyInputDetails = ValidatePolicyInputDetails(policyDto);

        if (validationPolicyInputDetails is not null)
        {
            return Result<PolicyDto>.Failure(validationPolicyInputDetails);
        }


        var validationPolicyReferences = await policyReferenceService.ValidatePolicyReferencesAsync(
            policyDto.ClientId,
            policyDto.BuildingId,
            policyDto.BrokerId,
            policyDto.CurrencyId,
            cancellationToken
        );

        if (!validationPolicyReferences.IsSuccess)
        {
            if (validationPolicyReferences.Error is null)
            {
                return Result<PolicyDto>.Failure(PolicyErrors.PolicyReferencesValidationFailed);
            }
            else
            {
                return Result<PolicyDto>.Failure(validationPolicyReferences.Error);
            }
        }

        if (validationPolicyReferences.Value is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.PolicyReferencesValidationFailed);
        }


        var overlappingPolicyExists = await policyRepository.CheckOverlappingPolicyExistsAsync(
            policy.BuildingId,
            PolicyStatus.Active,
            policy.StartDate,
            policy.EndDate,
            cancellationToken);

        if (overlappingPolicyExists)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.OverlappingPolicyExists);
        }

        policy.Status = PolicyStatus.Active;
        policy.ActivationDate = datetimeNow;
        policy.ModifiedAt = datetimeNow;

        await policyRepository.SavePolicyChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Policy {PolicyId} activated.", policyId);
        }

        return Result<PolicyDto>.Success(MapPolicyToDto(policy));
    }

    public async Task<Result<PolicyDto>> CancelPolicyAsync(Guid policyId, CancelPolicyDto cancelPolicyDto, CancellationToken cancellationToken)
    {
        if (policyId == Guid.Empty)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InvalidPolicyId);
        }

        if (string.IsNullOrWhiteSpace(cancelPolicyDto.CancellationReason))
        {
            return Result<PolicyDto>.Failure(PolicyErrors.CancellationReasonRequired);
        }

        if (cancelPolicyDto.CancellationReason.Length > PolicyConstraints.CancellationReasonMaxLength)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.InvalidCancellationReasonMaxLength);
        }

        var policy = await policyRepository.GetPolicyForUpdateAsync(policyId, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.NotFound(policyId));
        }

        if (policy.Status != PolicyStatus.Active)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.CancellationInactivePolicy);
        }

        if (policy.EndDate < DateTime.UtcNow.Date)
        {
            return Result<PolicyDto>.Failure(PolicyErrors.CancellationExpiredPolicy);
        }

        policy.Status = PolicyStatus.Cancelled;
        policy.CancellationDate = DateTime.UtcNow;
        policy.CancellationReason = cancelPolicyDto.CancellationReason;
        policy.ModifiedAt = DateTime.UtcNow;

        await policyRepository.SavePolicyChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Policy {PolicyId} has been cancelled.", policyId);
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

    private static Error? ValidatePolicyInputDetails(CreatePolicyDto dto)
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

        if (dto.StartDate.Date > dto.EndDate.Date)
        {
            return PolicyErrors.InvalidDateRange;
        }

        return null;
    }

}
