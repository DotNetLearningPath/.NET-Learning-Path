using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Policy;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class PolicyService(
        IPolicyRepository policyRepository,
        IFeeConfigurationRepository feeConfigurationRepository,
        IRiskFactorConfigurationRepository riskFactorConfigurationRepository,
        IClientRepository clientRepository,
        ICurrencyRepository currencyRepository,
        ICityRepository cityRepository,
        ICountyRepository countyRepository,
        IBuildingRepository buildingRepository,
        IGenericRepository<Broker> brokerRepository,
        IGenericRepository<Country> countryRepository,
        IMapper mapper,
        ILogger<PolicyService> logger) : IPolicyService
    {
        public async Task<PolicyDto> GetPolicyByIdAsync(Guid id)
        {
            var policy = await policyRepository.GetByIdWithSnapshotsAsync(id)
                ?? throw new EntityNotFoundException($"Policy '{id}' was not found.");
            return mapper.Map<PolicyDto>(policy);
        }

        public async Task<PagedResult<PolicyListItemDto>> GetAllPoliciesAsync(
            int pageNumber,
            int pageSize,
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateOnly? startDate,
            DateOnly? endDate)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            ValidatePolicyListFilters(clientId, brokerId, startDate, endDate);

            var page = await policyRepository.SearchAsync(
                clientId, brokerId, status, startDate, endDate, pageNumber, pageSize);

            return new PagedResult<PolicyListItemDto>
            {
                Items = mapper.Map<List<PolicyListItemDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto request)
        {
            ValidatePolicyValues(request.ClientId, request.BuildingId, request.BrokerId,
                request.CurrencyId, request.BasePremiumAmount, request.StartDate, request.EndDate);

            var references = await GetPolicyReferencesAsync(
                request.ClientId, request.BuildingId, request.BrokerId, request.CurrencyId);

            var policy = new Policy
            {
                PolicyNumber = $"POL-{Guid.NewGuid():N}".ToUpperInvariant(),
                ClientId = request.ClientId,
                BuildingId = request.BuildingId,
                BrokerId = request.BrokerId,
                CurrencyId = request.CurrencyId,
                Status = PolicyStatus.Draft,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                BasePremiumAmount = request.BasePremiumAmount
            };

            await CalculatePolicyAsync(policy, references);
            var createdPolicy = await policyRepository.AddAsync(policy);

            logger.LogInformation(
                "Created draft policy {PolicyId} ({PolicyNumber}) for broker {BrokerId}; final premium {FinalPremiumAmount}",
                createdPolicy.Id, createdPolicy.PolicyNumber, createdPolicy.BrokerId, createdPolicy.FinalPremiumAmount);
            return mapper.Map<PolicyDto>(createdPolicy);
        }

        public async Task<PolicyDto> ActivatePolicyAsync(Guid id)
        {
            var policy = await policyRepository.GetByIdWithSnapshotsAsync(id)
                ?? throw new EntityNotFoundException($"Policy '{id}' was not found.");

            if (policy.Status != PolicyStatus.Draft)
                throw new ConflictException("Only draft policies can be activated.");

            var broker = await brokerRepository.GetAsync(policy.BrokerId)
                ?? throw new EntityNotFoundException($"Broker '{policy.BrokerId}' was not found.");
            if (broker.Status != BrokerStatus.Active)
                throw new ConflictException("An inactive broker cannot activate a policy.");

            var currency = await currencyRepository.GetAsync(policy.CurrencyId)
                ?? throw new EntityNotFoundException($"Currency '{policy.CurrencyId}' was not found.");
            if (!currency.IsActive)
                throw new ConflictException("An inactive currency cannot be used for an active policy.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (policy.StartDate < today)
                throw new ValidationException("A policy cannot be activated before its start date.");

            policy.Status = PolicyStatus.Active;
            policy.ActivatedAt = DateTimeOffset.UtcNow;
            policy.UpdatedAt = policy.ActivatedAt;
            await policyRepository.SavePolicyChangesAsync();

            logger.LogInformation("Activated policy {PolicyId}", policy.Id);
            return mapper.Map<PolicyDto>(policy);
        }

        public async Task<PolicyDto> CancelPolicyAsync(Guid id, CancelPolicyDto request)
        {
            var policy = await policyRepository.GetByIdWithSnapshotsAsync(id)
                ?? throw new EntityNotFoundException($"Policy '{id}' was not found.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ValidationException("A cancellation reason is required.");
            if (request.Reason.Trim().Length > 500)
                throw new ValidationException("Cancellation reason cannot exceed 500 characters.");

            if (policy.Status != PolicyStatus.Active)
                throw new ConflictException("Only active policies can be cancelled.");
            if (request.EffectiveDate == DateOnly.MinValue)
                throw new ValidationException("A cancellation effective date is required.");
            if (request.EffectiveDate < policy.StartDate || request.EffectiveDate > policy.EndDate)
                throw new ValidationException("Cancellation effective date must be within the policy period.");

            policy.Status = PolicyStatus.Cancelled;
            policy.CancellationReason = request.Reason.Trim();
            policy.CancellationEffectiveDate = request.EffectiveDate;
            policy.UpdatedAt = DateTimeOffset.UtcNow;
            await policyRepository.SavePolicyChangesAsync();

            logger.LogInformation("Cancelled policy {PolicyId} effective {EffectiveDate}",
                policy.Id, policy.CancellationEffectiveDate);
            return mapper.Map<PolicyDto>(policy);
        }

        public async Task<PolicyDto> ExpirePolicyAsync(Guid id)
        {
            var policy = await policyRepository.GetByIdWithSnapshotsAsync(id)
                ?? throw new EntityNotFoundException($"Policy '{id}' was not found.");

            if (policy.Status != PolicyStatus.Active)
                throw new ConflictException("Only active policies can be marked as expired.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (policy.EndDate >= today)
                throw new ConflictException("A policy cannot expire before its end date has passed.");

            policy.Status = PolicyStatus.Expired;
            policy.UpdatedAt = DateTimeOffset.UtcNow;
            await policyRepository.SavePolicyChangesAsync();

            logger.LogInformation("Marked policy {PolicyId} as expired", policy.Id);
            return mapper.Map<PolicyDto>(policy);
        }

        private async Task<PolicyReferences> GetPolicyReferencesAsync(
            Guid clientId,
            Guid buildingId,
            Guid brokerId,
            Guid currencyId)
        {
            _ = await clientRepository.GetAsync(clientId)
                ?? throw new EntityNotFoundException($"Client '{clientId}' was not found.");

            var building = await buildingRepository.GetAsync(buildingId)
                ?? throw new EntityNotFoundException($"Building '{buildingId}' was not found.");
            if (building.ClientId != clientId)
                throw new ValidationException("The selected building does not belong to the selected client.");

            var broker = await brokerRepository.GetAsync(brokerId)
                ?? throw new EntityNotFoundException($"Broker '{brokerId}' was not found.");
            if (broker.Status != BrokerStatus.Active)
                throw new ConflictException("Inactive brokers cannot create policies.");

            var currency = await currencyRepository.GetAsync(currencyId)
                ?? throw new EntityNotFoundException($"Currency '{currencyId}' was not found.");
            if (!currency.IsActive)
                throw new ConflictException("Inactive currencies cannot be used for policies.");

            var city = await cityRepository.GetAsync(building.CityId)
                ?? throw new EntityNotFoundException("The building references a city that does not exist.");
            var county = await countyRepository.GetAsync(city.CountyId)
                ?? throw new EntityNotFoundException("The building's city references a county that does not exist.");
            var country = await countryRepository.GetAsync(county.CountryId)
                ?? throw new EntityNotFoundException("The building's county references a country that does not exist.");

            return new PolicyReferences(building, city, county, country);
        }

        private async Task CalculatePolicyAsync(Policy policy, PolicyReferences references)
        {
            var fees = await feeConfigurationRepository.GetApplicableFeeConfigurationsAsync(policy.StartDate);
            var riskFactors = await riskFactorConfigurationRepository.GetApplicableRiskFactorConfigurationsAsync(
                references.Country.Id,
                references.County.Id,
                references.City.Id,
                references.Building.Type);
            var capturedAt = DateTimeOffset.UtcNow;

            foreach (var fee in fees)
            {
                policy.CalculationSnapshotEntries.Add(new PolicyCalculationSnapshot
                {
                    PolicyId = policy.Id,
                    SnapshotType = PolicyCalculationSnapshotType.FeeConfiguration,
                    OriginalConfigurationId = fee.Id,
                    AppliedConfigurationName = fee.Name,
                    AppliedFeeConfigurationType = fee.Type,
                    AppliedPercentage = fee.Percentage,
                    CapturedAt = capturedAt
                });
            }

            foreach (var factor in riskFactors)
            {
                string targetName;

                if (factor.Level == RiskFactorLevel.Country)
                    targetName = references.Country.Name;
                else if (factor.Level == RiskFactorLevel.County)
                    targetName = references.County.Name;
                else if (factor.Level == RiskFactorLevel.City)
                    targetName = references.City.Name;
                else if (factor.Level == RiskFactorLevel.BuildingType)
                    targetName = references.Building.Type.ToString();
                else
                    throw new ValidationException("A risk factor has an unsupported level.");

                policy.CalculationSnapshotEntries.Add(new PolicyCalculationSnapshot
                {
                    PolicyId = policy.Id,
                    SnapshotType = PolicyCalculationSnapshotType.RiskFactorConfiguration,
                    OriginalConfigurationId = factor.Id,
                    AppliedConfigurationName = $"{factor.Level} risk factor",
                    AppliedRiskFactorLevel = factor.Level,
                    AppliedRiskFactorTargetName = targetName,
                    AppliedPercentage = factor.AdjustmentPercentage,
                    CapturedAt = capturedAt
                });
            }

            var totalPercentage = policy.CalculationSnapshotEntries
                .Sum(snapshot => snapshot.AppliedPercentage);

            var adjustmentAmount = policy.BasePremiumAmount * totalPercentage / 100m;
            var finalPremium = policy.BasePremiumAmount + adjustmentAmount;
            policy.FinalPremiumAmount = decimal.Round(
                finalPremium, 2, MidpointRounding.AwayFromZero);

            if (policy.FinalPremiumAmount <= 0)
                throw new ValidationException("The applied fee and risk adjustments must result in a positive final premium.");
        }

        private static void ValidatePolicyValues(
            Guid clientId,
            Guid buildingId,
            Guid brokerId,
            Guid currencyId,
            decimal basePremiumAmount,
            DateOnly startDate,
            DateOnly endDate)
        {
            if (clientId == Guid.Empty)
                throw new ValidationException("Client is required.");
            if (buildingId == Guid.Empty)
                throw new ValidationException("Building is required.");
            if (brokerId == Guid.Empty)
                throw new ValidationException("Broker is required.");
            if (currencyId == Guid.Empty)
                throw new ValidationException("Currency is required.");
            if (basePremiumAmount <= 0)
                throw new ValidationException("Base premium must be greater than zero.");
            if (startDate == DateOnly.MinValue || endDate == DateOnly.MinValue)
                throw new ValidationException("Policy start and end dates are required.");
            if (endDate <= startDate)
                throw new ValidationException("Policy end date must be after its start date.");
        }

        private static void ValidatePolicyListFilters(Guid? clientId, Guid? brokerId, DateOnly? startDate, DateOnly? endDate)
        {
            if (clientId == Guid.Empty || brokerId == Guid.Empty)
                throw new ValidationException("Client and broker filters must be valid identifiers.");
            if (startDate == DateOnly.MinValue || endDate == DateOnly.MinValue)
                throw new ValidationException("Policy date filters must be valid dates.");
            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
                throw new ValidationException("The end date filter must be on or after the start date filter.");
        }

        private sealed record PolicyReferences(
            Building Building,
            City City,
            County County,
            Country Country);
    }
}
