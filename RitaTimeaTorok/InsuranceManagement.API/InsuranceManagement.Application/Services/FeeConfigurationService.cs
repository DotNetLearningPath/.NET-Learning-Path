using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class FeeConfigurationService(
        IGenericRepository<FeeConfiguration> feeConfigurationRepository,
        IMapper mapper,
        ILogger<FeeConfigurationService> logger) : IFeeConfigurationService
    {
        public async Task<PagedResult<FeeConfigurationDto>> GetAllFeeConfigurationsAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await feeConfigurationRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved fee configurations page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} configurations",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<FeeConfigurationDto>
            {
                Items = mapper.Map<List<FeeConfigurationDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<FeeConfigurationDto> CreateFeeConfigurationAsync(CreateFeeConfigurationDto request)
        {
            ValidateConfiguration(
                request.Name,
                request.Type,
                request.Percentage,
                request.EffectiveFrom,
                request.EffectiveTo);

            var configuration = mapper.Map<FeeConfiguration>(request);
            configuration.Name = request.Name.Trim();
            configuration.Type = request.Type.Trim();
            var createdConfiguration = await feeConfigurationRepository.AddAsync(configuration);

            logger.LogInformation("Created fee configuration {ConfigurationId}", createdConfiguration.Id);
            return mapper.Map<FeeConfigurationDto>(createdConfiguration);
        }

        public async Task<FeeConfigurationDto> UpdateFeeConfigurationAsync(
            Guid id,
            UpdateFeeConfigurationDto request)
        {
            var configuration = await feeConfigurationRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Fee configuration '{id}' was not found.");

            ValidateConfiguration(
                request.Name,
                request.Type,
                request.Percentage,
                request.EffectiveFrom,
                request.EffectiveTo);

            configuration.Name = request.Name.Trim();
            configuration.Type = request.Type.Trim();
            configuration.Percentage = request.Percentage;
            configuration.EffectiveFrom = request.EffectiveFrom;
            configuration.EffectiveTo = request.EffectiveTo;
            configuration.IsActive = request.IsActive;

            await feeConfigurationRepository.UpdateAsync(configuration);
            logger.LogInformation(
                "Updated fee configuration {ConfigurationId}; active: {IsActive}",
                configuration.Id,
                configuration.IsActive);
            return mapper.Map<FeeConfigurationDto>(configuration);
        }

        private static void ValidateConfiguration(
            string name,
            string type,
            decimal percentage,
            DateOnly effectiveFrom,
            DateOnly? effectiveTo)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Fee configuration name is required.");
            if (name.Trim().Length > 200)
                throw new ValidationException("Fee configuration name cannot exceed 200 characters.");
            if (string.IsNullOrWhiteSpace(type))
                throw new ValidationException("Fee configuration type is required.");
            if (type.Trim().Length > 50)
                throw new ValidationException("Fee configuration type cannot exceed 50 characters.");
            if (percentage < 0 || percentage > 300)
                throw new ValidationException("Percentage must be between 0 and 300.");
            if (effectiveFrom == DateOnly.MinValue)
                throw new ValidationException("Effective from date is required.");
            if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
                throw new ValidationException("Effective to date cannot be earlier than effective from date.");
        }
    }
}
