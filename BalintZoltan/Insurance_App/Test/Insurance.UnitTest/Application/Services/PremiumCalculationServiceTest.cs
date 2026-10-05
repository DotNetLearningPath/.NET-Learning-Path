using Insurance.Application.DTO.Premiums;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class PremiumCalculationServiceTest
{
    private readonly FakeFeeRepository _feeRepository;
    private readonly FakeRiskFactorRepository _riskFactorRepository;
    private readonly PremiumCalculationService _service;
    private readonly DateTime _effectiveAt;

    public PremiumCalculationServiceTest()
    {
        _feeRepository = new FakeFeeRepository();
        _riskFactorRepository = new FakeRiskFactorRepository();
        _service = new PremiumCalculationService(
            _feeRepository,
            _riskFactorRepository);
        _effectiveAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public async Task CalculatesBasePremiumWhenNoFeesOrRiskFactorsApply()
    {
        var result = await _service.CalculateFinalPremiumAsync(
            125m,
            _effectiveAt,
            new PremiumCalculationContext());

        Assert.Equal(125m, result);
    }

    [Fact]
    public async Task AddsPercentagesFromAllFeeTypes()
    {
        _feeRepository.Storage.AddRange(
        [
            CreateFee("Broker commission", FeeType.BrokerCommission, 2.5m),
            CreateFee("Risk fee", FeeType.RiskAdjustment, 1.5m),
            CreateFee("Admin fee", FeeType.AdminFee, 7m)
        ]);

        var result = await _service.CalculateFinalPremiumAsync(
            100m,
            _effectiveAt,
            new PremiumCalculationContext());

        Assert.Equal(111m, result);
    }

    [Fact]
    public async Task AddsApplicablePositiveAndNegativeRiskAdjustmentsToFeePercentages()
    {
        var countryId = Guid.NewGuid();
        var countyId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var buildingType = BuildingType.Historic;

        _feeRepository.Storage.Add(
            CreateFee("Broker commission", FeeType.BrokerCommission, 2.5m));
        _riskFactorRepository.Storage.AddRange(
        [
            CreateRiskFactor(RiskFactorLevel.Country, countryId.ToString(), 1m),
            CreateRiskFactor(RiskFactorLevel.County, countyId.ToString(), 5m),
            CreateRiskFactor(RiskFactorLevel.City, cityId.ToString(), 8m),
            CreateRiskFactor(RiskFactorLevel.BuildingType, buildingType.ToString(), -2m),
            CreateRiskFactor(RiskFactorLevel.City, Guid.NewGuid().ToString(), 40m),
            CreateRiskFactor(RiskFactorLevel.BuildingType, BuildingType.Residential.ToString(), 30m),
            CreateRiskFactor(RiskFactorLevel.City, cityId.ToString(), 50m, isActive: false)
        ]);

        var context = new PremiumCalculationContext
        {
            CountryId = countryId,
            CountyId = countyId,
            CityId = cityId,
            BuildingType = buildingType
        };

        var result = await _service.CalculateFinalPremiumAsync(
            100m,
            _effectiveAt,
            context);

        Assert.Equal(114.5m, result);
    }

    [Fact]
    public async Task IgnoresInactiveAndOutOfDateFeesAndInactiveRiskFactors()
    {
        var cityId = Guid.NewGuid();
        _feeRepository.Storage.AddRange(
        [
            CreateFee("Inactive fee", FeeType.BrokerCommission, 50m, isActive: false),
            CreateFee("Expired fee", FeeType.AdminFee, 25m, effectiveTo: _effectiveAt.AddDays(-1)),
            CreateFee("Future fee", FeeType.RiskAdjustment, 25m, effectiveFrom: _effectiveAt.AddDays(1))
        ]);
        _riskFactorRepository.Storage.Add(
            CreateRiskFactor(
                RiskFactorLevel.City,
                cityId.ToString(),
                20m,
                isActive: false));

        var context = new PremiumCalculationContext
        {
            CityId = cityId
        };

        var result = await _service.CalculateFinalPremiumAsync(
            100m,
            _effectiveAt,
            context);

        Assert.Equal(100m, result);
    }

    [Fact]
    public async Task AppliesFeeStartingOnThePolicyEffectiveDate()
    {
        var policyStartDate = _effectiveAt.AddDays(30);
        _feeRepository.Storage.Add(
            CreateFee(
                "Scheduled fee",
                FeeType.AdminFee,
                4m,
                effectiveFrom: policyStartDate));

        var result = await _service.CalculateFinalPremiumAsync(
            100m,
            policyStartDate,
            new PremiumCalculationContext());

        Assert.Equal(104m, result);
    }

    [Fact]
    public async Task RejectsNegativeBasePremium()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.CalculateFinalPremiumAsync(
                -1m,
                _effectiveAt,
                new PremiumCalculationContext()));
    }

    [Fact]
    public async Task RejectsNullCalculationContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CalculateFinalPremiumAsync(
                100m,
                _effectiveAt,
                null!));
    }

    static private FeeConfiguration CreateFee(
        string name,
        FeeType type,
        decimal percentage,
        bool isActive = true,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null)
    {
        return new FeeConfiguration(
            name,
            type,
            percentage,
            effectiveFrom ?? DateTime.UnixEpoch,
            effectiveTo,
            isActive);
    }

    private static RiskFactorConfiguration CreateRiskFactor(
        RiskFactorLevel level,
        string reference,
        decimal adjustmentPercentage,
        bool isActive = true)
    {
        return new RiskFactorConfiguration(
            level,
            reference,
            adjustmentPercentage,
            isActive);
    }
}
