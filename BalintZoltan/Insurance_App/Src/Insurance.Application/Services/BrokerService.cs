using System.ComponentModel.DataAnnotations;
using Application.Abstractions;
using Application.DTO.Brokers;
using Application.DTO.Common;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public sealed class BrokerService : IBrokerService
{
    private readonly IBrokerRepository _brokerRepository;

    public BrokerService(IBrokerRepository brokerRepository) => _brokerRepository = brokerRepository;

    public async Task<BrokerDto> CreateBrokerAsync(CreateBrokerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.BrokerCode, request.Name, request.Email, request.Phone, request.CommissionPercentage);
        await EnsureCodeIsUniqueAsync(request.BrokerCode, null, cancellationToken);

        var broker = new Broker(
            request.BrokerCode.Trim(),
            request.Name.Trim(),
            request.Email.Trim(),
            request.Phone.Trim(),
            commissionPercentage: request.CommissionPercentage);
        await _brokerRepository.AddBrokerAsync(broker, cancellationToken);
        return Map(broker);
    }

    public async Task<BrokerDto?> GetBrokerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var broker = await _brokerRepository.GetBrokerByIdAsync(id, cancellationToken);
        return broker is null ? null : Map(broker);
    }

    public async Task<PagedResult<BrokerDto>> ListBrokersAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var result = await _brokerRepository.ListBrokersAsync(pagination, cancellationToken);
        return new PagedResult<BrokerDto>
        {
            Items = result.Items.Select(Map).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.BrokerCode, request.Name, request.Email, request.Phone, request.CommissionPercentage);
        var broker = await GetRequiredBrokerAsync(id, cancellationToken);
        await EnsureCodeIsUniqueAsync(request.BrokerCode, id, cancellationToken);

        broker.Update(request.BrokerCode, request.Name, request.Email, request.Phone, request.CommissionPercentage);
        await _brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        return Map(broker);
    }

    public Task<BrokerDto> ActivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        SetStatusAsync(id, true, cancellationToken);

    public Task<BrokerDto> DeactivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        SetStatusAsync(id, false, cancellationToken);

    private async Task<BrokerDto> SetStatusAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        var broker = await GetRequiredBrokerAsync(id, cancellationToken);
        if (active)
        {
            broker.Activate();
        }
        else
        {
            broker.Deactivate();
        }
        await _brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        return Map(broker);
    }

    private async Task<Broker> GetRequiredBrokerAsync(Guid id, CancellationToken cancellationToken) =>
        await _brokerRepository.GetBrokerByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Broker was not found.");

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await _brokerRepository.ExistsBrokerByCodeAsync(code.Trim(), excludedId, cancellationToken))
        {
            throw new InvalidOperationException("A broker with this code already exists.");
        }
    }

    private static void ValidateRequest(string code, string name, string email, string phone, decimal? commissionPercentage)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Broker code is required.");
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Broker name is required.");
        }
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Broker phone is required.");
        }
        if (code.Trim().Length > 50)
        {
            throw new ArgumentException("Broker code cannot be longer than 50 characters.");
        }
        if (name.Trim().Length > 200)
        {
            throw new ArgumentException("Broker name cannot be longer than 200 characters.");
        }
        if (phone.Trim().Length > 50)
        {
            throw new ArgumentException("Broker phone cannot be longer than 50 characters.");
        }
        if (commissionPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(commissionPercentage));
        }
        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > 254 || !new EmailAddressAttribute().IsValid(email.Trim()))
        {
            throw new ArgumentException("Invalid email address format.", nameof(email));
        }
    }

    private static BrokerDto Map(Broker broker) => new()
    {
        Id = broker.Id, BrokerCode = broker.BrokerCode, Name = broker.Name,
        Email = broker.Email, Phone = broker.Phone, Status = broker.Status,
        CommissionPercentage = broker.CommissionPercentage
    };
}
