using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Brokers;
using Insurance.Application.DTO.Common;
using Insurance.Application.Exceptions;
using Insurance.Application.Validation;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public sealed class BrokerService(
    IBrokerRepository brokerRepository) : IBrokerService
{
    private const int MaxBrokerCodeLength = 50;
    private const int MaxPhoneLength = 50;
    private const int MaxNameLength = 200;
    private const int MaxCommissionPercentage = 100;
    private const int MinCommissionPercentage = 0;

    public async Task<BrokerDto> CreateBrokerAsync(CreateBrokerRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var broker = new Broker(
            request.BrokerCode.Trim(),
            request.Name.Trim(),
            request.Email.Trim(),
            request.Phone.Trim(),
            commissionPercentage: request.CommissionPercentage);
        await brokerRepository.AddBrokerAsync(broker, cancellationToken);
        return MapToBrokerDto(broker);
    }

    public async Task<BrokerDto?> GetBrokerByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var broker = await brokerRepository.GetBrokerByIdAsync(id, cancellationToken);
        return broker is null ? null : MapToBrokerDto(broker);
    }

    public async Task<PagedResult<BrokerDto>> ListBrokersAsync(PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await brokerRepository.ListBrokersAsync(pagination, cancellationToken);
        return new PagedResult<BrokerDto>
        {
            Items = [.. result.Items.Select(MapToBrokerDto)],
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var broker = await GetRequiredBrokerAsync(id, cancellationToken);
        broker.Update(request.BrokerCode, request.Name, request.Email, request.Phone, request.CommissionPercentage);
        await brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        return MapToBrokerDto(broker);
    }

    public Task<BrokerDto> ActivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        SetStatusAsync(id, true, cancellationToken);

    public Task<BrokerDto> DeactivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken) =>
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
        await brokerRepository.UpdateBrokerAsync(broker, cancellationToken);
        return MapToBrokerDto(broker);
    }

    private async Task<Broker> GetRequiredBrokerAsync(Guid id, CancellationToken cancellationToken) =>
        await brokerRepository.GetBrokerByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Broker was not found.");

    private static void ValidateRequest(IBrokerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.BrokerCode))
        {
            throw new ArgumentException("Broker code is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Broker name is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ArgumentException("Broker phone is required.");
        }
        if (request.BrokerCode.Trim().Length > MaxBrokerCodeLength)
        {
            throw new ArgumentException("Broker code cannot be longer than 50 characters.");
        }
        if (request.Name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException("Broker name cannot be longer than 200 characters.");
        }
        if (request.Phone.Trim().Length > MaxPhoneLength)
        {
            throw new ArgumentException("Broker phone cannot be longer than 50 characters.");
        }
        if (request.CommissionPercentage is < MinCommissionPercentage or > MaxCommissionPercentage)
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Broker email is required.", nameof(request));
        }

        EmailValidator.ValidateFormatAndLength(request.Email);
    }

    private static BrokerDto MapToBrokerDto(Broker broker) => new()
    {
        Id = broker.Id,
        BrokerCode = broker.BrokerCode,
        Name = broker.Name,
        Email = broker.Email,
        Phone = broker.Phone,
        Status = broker.Status,
        CommissionPercentage = broker.CommissionPercentage
    };
}
