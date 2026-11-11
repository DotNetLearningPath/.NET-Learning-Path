using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class RiskFactorConfigurationService(
        IGenericRepository<RiskFactorConfiguration> riskFactorConfigurationRepository,
        IGenericRepository<Country> countryRepository,
        IGenericRepository<County> countyRepository,
        IGenericRepository<City> cityRepository,
        IMapper mapper,
        ILogger<RiskFactorConfigurationService> logger) : IRiskFactorConfigurationService
    {
        public async Task<PagedResult<RiskFactorConfigurationDto>> GetAllRiskFactorConfigurationsAsync(
            int pageNumber,
            int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await riskFactorConfigurationRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved risk factor configurations page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} configurations",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<RiskFactorConfigurationDto>
            {
                Items = mapper.Map<List<RiskFactorConfigurationDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<RiskFactorConfigurationDto> CreateRiskFactorConfigurationAsync(
            CreateRiskFactorConfigurationDto request)
        {
            await ValidateTargetAsync(
                request.Level,
                request.CountryId,
                request.CountyId,
                request.CityId,
                request.BuildingType);
            ValidateAdjustmentPercentage(request.AdjustmentPercentage);

            var configuration = mapper.Map<RiskFactorConfiguration>(request);
            var createdConfiguration = await riskFactorConfigurationRepository.AddAsync(configuration);

            logger.LogInformation("Created risk factor configuration {ConfigurationId}", createdConfiguration.Id);
            return mapper.Map<RiskFactorConfigurationDto>(createdConfiguration);
        }

        public async Task<RiskFactorConfigurationDto> UpdateRiskFactorConfigurationAsync(
            Guid id,
            UpdateRiskFactorConfigurationDto request)
        {
            var configuration = await riskFactorConfigurationRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Risk factor configuration '{id}' was not found.");

            await ValidateTargetAsync(
                request.Level,
                request.CountryId,
                request.CountyId,
                request.CityId,
                request.BuildingType);
            ValidateAdjustmentPercentage(request.AdjustmentPercentage);

            mapper.Map(request, configuration);

            await riskFactorConfigurationRepository.UpdateAsync(configuration);
            logger.LogInformation(
                "Updated risk factor configuration {ConfigurationId}; active: {IsActive}",
                configuration.Id,
                configuration.IsActive);
            return mapper.Map<RiskFactorConfigurationDto>(configuration);
        }

        private async Task ValidateTargetAsync(
            RiskFactorLevel level,
            Guid? countryId,
            Guid? countyId,
            Guid? cityId,
            BuildingType? buildingType)
        {
            int targetCount = 0;

            if (countryId.HasValue) targetCount++;
            if (countyId.HasValue) targetCount++;
            if (cityId.HasValue) targetCount++;
            if (buildingType.HasValue) targetCount++;

            if (targetCount != 1)
                throw new ValidationException("Exactly one risk factor target must be provided.");

            switch (level)
            {
                case RiskFactorLevel.Country:
                    if (!countryId.HasValue)
                        throw new ValidationException("CountryId is required for country risk factors.");

                    if (await countryRepository.GetAsync(countryId.Value) is null)
                        throw new EntityNotFoundException($"Country '{countryId.Value}' was not found.");
                    break;

                case RiskFactorLevel.County:
                    if (!countyId.HasValue)
                        throw new ValidationException("CountyId is required for county risk factors.");

                    if (await countyRepository.GetAsync(countyId.Value) is null)
                        throw new EntityNotFoundException($"County '{countyId.Value}' was not found.");
                    break;

                case RiskFactorLevel.City:
                    if (!cityId.HasValue)
                        throw new ValidationException("CityId is required for city risk factors.");

                    if (await cityRepository.GetAsync(cityId.Value) is null)
                        throw new EntityNotFoundException($"City '{cityId.Value}' was not found.");
                    break;

                case RiskFactorLevel.BuildingType:
                    if (!buildingType.HasValue)
                        throw new ValidationException("BuildingType is required for building type risk factors.");

                    if (!Enum.IsDefined(buildingType.Value))
                        throw new ValidationException("Building type is invalid.");
                    break;

                default:
                    throw new ValidationException("Risk factor level is invalid.");
            }
        }

        private static void ValidateAdjustmentPercentage(decimal adjustmentPercentage)
        {
            if (adjustmentPercentage < -50 || adjustmentPercentage > 300)
                throw new ValidationException("Adjustment percentage must be between -50 and 300.");
        }

    }
}
