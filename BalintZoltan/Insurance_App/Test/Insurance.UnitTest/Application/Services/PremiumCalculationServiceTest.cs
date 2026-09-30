using Application.DTO.Common;
using Insurance.Application.Services;
using Insurance.Application.Abstractions;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Application.Services;

public sealed class PremiumCalculationServiceTest
{
    [Fact]
    public async Task CalculatesBasePremiumWhenNoFeesAreActive()
    {
        var service = new PremiumCalculationService(new FeeRepository([]));
        Assert.Equal(125m, await service.CalculateFinalPremiumAsync(125m, DateTime.UtcNow));
    }

    [Fact]
    public async Task AddsAllPercentageAdjustmentsAndFixedFees()
    {
        var fees = new[]
        {
            new FeeConfiguration("Broker", FeeType.BrokerCommission, 2.5m, DateTime.UnixEpoch),
            new FeeConfiguration("Risk adjustment", FeeType.RiskAdjustment, 1.5m, DateTime.UnixEpoch),
            new FeeConfiguration("Admin fee", FeeType.AdminFee, 7m, DateTime.UnixEpoch)
        };
        var service = new PremiumCalculationService(new FeeRepository(fees));
        Assert.Equal(111m, await service.CalculateFinalPremiumAsync(100m, DateTime.UtcNow));
    }

    [Fact]
    public async Task IgnoresInactiveAndOutOfDateConfigurations()
    {
        var at = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var fees = new[]
        {
            new FeeConfiguration("Inactive", FeeType.BrokerCommission, 50m, DateTime.UnixEpoch, isActive: false),
            new FeeConfiguration("Expired", FeeType.AdminFee, 25m, DateTime.UnixEpoch, at.AddDays(-1)),
            new FeeConfiguration("Future", FeeType.RiskAdjustment, 25m, at.AddDays(1))
        };
        var service = new PremiumCalculationService(new FeeRepository(fees));
        Assert.Equal(100m, await service.CalculateFinalPremiumAsync(100m, at));
    }

    [Fact]
    public async Task RejectsNegativeBasePremium()
    {
        var service = new PremiumCalculationService(new FeeRepository([]));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CalculateFinalPremiumAsync(-1m, DateTime.UtcNow));
    }

    private sealed class FeeRepository(IReadOnlyCollection<FeeConfiguration> fees) : IFeeConfigurationRepository
    {
        public Task<IReadOnlyCollection<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
            DateTime effectiveAt,
            CancellationToken cancellationToken = default)
        {
            var activeFees = fees
                .Where(fee => fee.IsActive
                    && fee.EffectiveFrom <= effectiveAt
                    && (!fee.EffectiveTo.HasValue || fee.EffectiveTo.Value >= effectiveAt))
                .ToList();

            return Task.FromResult<IReadOnlyCollection<FeeConfiguration>>(activeFees);
        }

        public Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
            FeeType type,
            DateTime effectiveAt,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<FeeConfiguration?>(null);
        }

        public Task AddFeeConfigurationAsync(
            FeeConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<FeeConfiguration?> GetFeeConfigurationByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<FeeConfiguration?>(null);
        }

        public Task UpdateFeeConfigurationAsync(
            FeeConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeactivateFeeConfigurationAsync(
            FeeConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
