using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Broker;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class BrokerService(
        IGenericRepository<Broker> brokerRepository,
        IMapper mapper,
        ILogger<BrokerService> logger) : IBrokerService
    {
        public async Task<PagedResult<BrokerDto>> GetAllBrokersAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await brokerRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved brokers page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} brokers",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<BrokerDto>
            {
                Items = mapper.Map<List<BrokerDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<BrokerDto> GetBrokerByIdAsync(Guid id)
        {
            var broker = await brokerRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Broker '{id}' was not found.");

            logger.LogInformation("Retrieved broker {BrokerId}", id);
            return mapper.Map<BrokerDto>(broker);
        }

        public async Task<BrokerDto> CreateBrokerAsync(CreateBrokerDto request)
        {
            ValidateBroker(request.BrokerCode, request.Name, request.Email,
                request.PhoneNumber, request.CommissionPercentage);

            var brokerCode = request.BrokerCode.Trim().ToUpperInvariant();
            var email = request.Email.Trim().ToLowerInvariant();

            var broker = mapper.Map<Broker>(request);
            broker.BrokerCode = brokerCode;
            broker.Name = request.Name.Trim();
            broker.Email = email;
            broker.PhoneNumber = request.PhoneNumber.Trim();

            var createdBroker = await brokerRepository.AddAsync(broker);
            logger.LogInformation("Created broker {BrokerId} with code {BrokerCode}",
                createdBroker.Id, createdBroker.BrokerCode);
            return mapper.Map<BrokerDto>(createdBroker);
        }

        public async Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerDto request)
        {
            ValidateBroker(request.BrokerCode, request.Name, request.Email,
                request.PhoneNumber, request.CommissionPercentage);

            var broker = await brokerRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Broker '{id}' was not found.");

            var brokerCode = request.BrokerCode.Trim().ToUpperInvariant();
            var email = request.Email.Trim().ToLowerInvariant();

            broker.BrokerCode = brokerCode;
            broker.Name = request.Name.Trim();
            broker.Email = email;
            broker.PhoneNumber = request.PhoneNumber.Trim();
            broker.CommissionPercentage = request.CommissionPercentage;
            broker.Status = request.Status;

            await brokerRepository.UpdateAsync(broker);
            logger.LogInformation("Updated broker {BrokerId}; status: {BrokerStatus}", broker.Id, broker.Status);
            return mapper.Map<BrokerDto>(broker);
        }

        private static void ValidateBroker(
            string brokerCode,
            string name,
            string email,
            string phoneNumber,
            decimal? commissionPercentage)
        {
            if (string.IsNullOrWhiteSpace(brokerCode))
                throw new ValidationException("Broker code is required.");
            if (brokerCode.Trim().Length > 30)
                throw new ValidationException("Broker code cannot exceed 30 characters.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Broker name is required.");
            if (name.Trim().Length > 200)
                throw new ValidationException("Broker name cannot exceed 200 characters.");
            if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email.Trim()))
            if (phoneNumber.Trim().Length > 30)
                throw new ValidationException("Broker phone number cannot exceed 30 characters.");
            if (commissionPercentage.HasValue &&
                (commissionPercentage.Value < 0 || commissionPercentage.Value > 100))
                throw new ValidationException("Commission percentage must be between 0 and 100.");
        }
    }
}
