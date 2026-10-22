using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.Models.Persistence;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class PolicyCalculationServiceTests
{
    private readonly Mock<IFeeConfigRepository> _feeConfigRepositoryMock;
    private readonly Mock<IRiskFactorConfigRepository> _riskFactorConfigRepositoryMock;
    private readonly Mock<IGeographyRepository> _geographyRepositoryMock;

    private readonly PolicyCalculationService _service;

    public PolicyCalculationServiceTests()
    {
        _feeConfigRepositoryMock = new Mock<IFeeConfigRepository>();
        _riskFactorConfigRepositoryMock = new Mock<IRiskFactorConfigRepository>();
        _geographyRepositoryMock = new Mock<IGeographyRepository>();

        _service = new PolicyCalculationService(
            _feeConfigRepositoryMock.Object,
            _riskFactorConfigRepositoryMock.Object,
            _geographyRepositoryMock.Object);
    }

    #region Calculate final premium
    [Theory]
    [InlineData(100, 106.75)]
    [InlineData(1000, 1067.50)]
    [InlineData(100.55, 107.34)]
    public async Task CalculateFinalPremiumAsync_WhenValidInput_ReturnsSuccess(decimal basePremium, decimal expectedFinalPremium)
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var geographyDetails = TestData.CreateGeographyDetails(building.CityId);
        var feeConfigs = new List<FeeConfig> { TestData.CreateFeeConfig1(), TestData.CreateFeeConfig2() };
        var riskFactorConfigs = new List<RiskFactorConfig> { TestData.CreateRiskFactorConfig1(), TestData.CreateRiskFactorConfig2() };
        var policyStartDate = DateTime.UtcNow.Date.AddDays(1);

        SetupGeographyDetails(building, geographyDetails);
        SetupFeeConfigs(policyStartDate, feeConfigs);
        SetupRiskFactorConfigs(building, geographyDetails, riskFactorConfigs);

        // Act & Assert
        await AssertSuccessCalculateFinalPremiumAsync(basePremium, building, geographyDetails, policyStartDate, expectedFinalPremium);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(2000.50)]
    public async Task CalculateFinalPremiumAsync_NoConfigurations_ReturnsBasePremium(decimal basePremium)
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var geographyDetails = TestData.CreateGeographyDetails(building.CityId);
        var policyStartDate = DateTime.UtcNow.Date.AddDays(1);

        SetupGeographyDetails(building, geographyDetails);
        SetupFeeConfigs(policyStartDate, []);
        SetupRiskFactorConfigs(building, geographyDetails, []);

        var expectedFinalPremium = basePremium;

        // Act & Assert
        await AssertSuccessCalculateFinalPremiumAsync(basePremium, building, geographyDetails, policyStartDate, expectedFinalPremium);
    }

    [Fact]
    public async Task CalculateFinalPremiumAsync_CityNotFound_ReturnsNotFound()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var policyStartDate = DateTime.UtcNow.Date.AddDays(1);
        var basePremium = 1000m;

        _geographyRepositoryMock.Setup(x => x.GetGeographyDetailsByCityAsync(building.CityId, It.IsAny<CancellationToken>())).ReturnsAsync((GeographyDetails?)null);

        // Act
        var result = await _service.CalculateFinalPremiumAsync(basePremium, building, policyStartDate, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(GeographyErrors.CityNotFound(building.CityId).Type, result.Error.Type);
        Assert.Equal(GeographyErrors.CityNotFound(building.CityId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.GetGeographyDetailsByCityAsync(building.CityId, It.IsAny<CancellationToken>()), Times.Once);
        _feeConfigRepositoryMock.Verify(x => x.GetActiveFeeConfigsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
        _riskFactorConfigRepositoryMock.Verify(x => x.GetActiveRiskFactorConfigsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion


    #region Helpers

    private void SetupGeographyDetails(Building building, GeographyDetails geographyDetails)
    {
        _geographyRepositoryMock.Setup(x => x.GetGeographyDetailsByCityAsync(building.CityId, It.IsAny<CancellationToken>())).ReturnsAsync(geographyDetails);
    }

    private void SetupFeeConfigs(DateTime policyStartDate, IReadOnlyList<FeeConfig> feeConfigs)
    {
        _feeConfigRepositoryMock.Setup(x => x.GetActiveFeeConfigsAsync(policyStartDate, null, It.IsAny<CancellationToken>())).ReturnsAsync(feeConfigs);
    }

    private void SetupRiskFactorConfigs(Building building, GeographyDetails geographyDetails, IReadOnlyList<RiskFactorConfig> riskFactorConfigs)
    {
        _riskFactorConfigRepositoryMock.Setup(x =>
            x.GetActiveRiskFactorConfigsAsync(
                geographyDetails.CountryId,
                geographyDetails.CountyId,
                geographyDetails.CityId,
                building.BuildingTypeId,
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(riskFactorConfigs);
    }

    private async Task AssertSuccessCalculateFinalPremiumAsync(decimal basePremium, Building building, GeographyDetails geographyDetails, DateTime policyStartDate, decimal expectedFinalPremium)
    {
        // Act
        var result = await _service.CalculateFinalPremiumAsync(basePremium, building, policyStartDate, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFinalPremium, result.Value);

        _geographyRepositoryMock.Verify(x => x.GetGeographyDetailsByCityAsync(building.CityId, It.IsAny<CancellationToken>()), Times.Once);
        _feeConfigRepositoryMock.Verify(x => x.GetActiveFeeConfigsAsync(policyStartDate, null, It.IsAny<CancellationToken>()), Times.Once);
        _riskFactorConfigRepositoryMock.Verify(x => x.GetActiveRiskFactorConfigsAsync(
            geographyDetails.CountryId,
            geographyDetails.CountyId,
            geographyDetails.CityId,
            building.BuildingTypeId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

}
