using Application.DTO.Common;
using Insurance.Application.Abstractions;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeFeeRepository : IFeeConfigurationRepository
{
    public readonly List<FeeConfiguration> Storage = new();

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public int DeactivateCallCount { get; private set; }

    public Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        Storage.Add(configuration);
        AddCallCount++;
        return Task.CompletedTask;
    }

    public Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken = default)
    {
        var fee = Storage.FirstOrDefault(configuration =>
            configuration.Type == type
            && configuration.IsActive
            && configuration.EffectiveFrom <= effectiveAt
            && (!configuration.EffectiveTo.HasValue
                || configuration.EffectiveTo.Value >= effectiveAt));

        return Task.FromResult(fee);
    }

    public Task<IReadOnlyCollection<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
        DateTime effectiveAt,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<FeeConfiguration> activeFees = Storage
            .Where(configuration => configuration.IsActive
                && configuration.EffectiveFrom <= effectiveAt
                && (!configuration.EffectiveTo.HasValue
                    || configuration.EffectiveTo.Value >= effectiveAt))
            .ToList();

        return Task.FromResult(activeFees);
    }

    public Task<FeeConfiguration?> GetFeeConfigurationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fee = Storage.FirstOrDefault(configuration => configuration.Id == id);
        return Task.FromResult(fee);
    }

    public Task UpdateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        return Task.CompletedTask;
    }

    public Task DeactivateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        configuration.Deactivate();
        DeactivateCallCount++;
        return Task.CompletedTask;
    }

    public Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var allFees = Storage
            .OrderBy(configuration => configuration.Name)
            .ThenByDescending(configuration => configuration.EffectiveFrom)
            .ThenBy(configuration => configuration.Id)
            .ToList();

        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);

        return Task.FromResult(new PagedResult<FeeConfiguration>
        {
            Items = allFees
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = allFees.Count
        });
    }
}
