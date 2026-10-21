using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Globalization;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class PolicyReportServiceTests
{
    private readonly Mock<IPolicyReportRepository> _repositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly PolicyReportService _service;

    public PolicyReportServiceTests()
    {
        _repositoryMock = new Mock<IPolicyReportRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(x => x["CurrencySettings:BaseCurrencyCode"]).Returns(TestData.BaseCurrencyCode);

        _service = new PolicyReportService(_repositoryMock.Object, _configurationMock.Object);
    }

    #region Reports policies by group type

    [Theory]
    [InlineData(PolicyReportGroupType.Country, "2026-10-01", "2027-10-01", PolicyStatus.Draft, "EUR", "Residential")]
    [InlineData(PolicyReportGroupType.County, "2026-10-01", null, PolicyStatus.Active, "RON", "Office")]
    [InlineData(PolicyReportGroupType.City, null, "2027-10-01", PolicyStatus.Active, "RON", "Office")]
    [InlineData(PolicyReportGroupType.Broker, "2026-10-01", "2026-10-01", PolicyStatus.Active, "RON", "Office")]
    [InlineData(PolicyReportGroupType.Country, null, null, null, null, null)]
    [InlineData(PolicyReportGroupType.County, null, null, null, null, null)]
    [InlineData(PolicyReportGroupType.City, null, null, null, null, null)]
    [InlineData(PolicyReportGroupType.Broker, null, null, null, null, null)]
    public async Task GetReportPoliciesAsync_ValidFilters_ReturnsSuccess(
        PolicyReportGroupType groupType,
        string? from,
        string? to, 
        PolicyStatus? status, 
        string? currency, 
        string? buildingType
    )
    {
        // Arrange
        DateTime? fromDate = from is null ? null : DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime? toDate = to is null ? null : DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var filters = new PolicyReportFiltersDto(fromDate, toDate, status?.ToString(), currency, buildingType);

        var reports = TestData.CreatePolicyReports(groupType);

        _repositoryMock.Setup(x => x.GetReportPoliciesAsync(groupType, filters, It.IsAny<CancellationToken>())).ReturnsAsync(reports);

        // Act
        var result = await _service.GetReportPoliciesAsync(groupType, filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(reports.Count, result.Value.Count);
        Assert.Equal(1000.13m, result.Value[0].TotalFinalPremium);
        Assert.Equal(5100.56m, result.Value[0].TotalFinalPremiumInBaseCurrency);
        Assert.Equal(5000.00m, result.Value[1].TotalFinalPremium);
        Assert.Equal(6000.25m, result.Value[1].TotalFinalPremiumInBaseCurrency);
        Assert.All(result.Value, report =>
        {
            Assert.Equal(groupType, report.GroupType);
            Assert.Equal(TestData.BaseCurrencyCode, report.BaseCurrencyCode);
        });

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(groupType, filters, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReportPoliciesAsync_NoMatchingPolicies_ReturnsEmptyList()
    {
        // Arrange
        var filters = new PolicyReportFiltersDto(null, null, null, "ABC", null);

        var reports = new List<PolicyReportDto>();

        _repositoryMock.Setup(x => x.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, It.IsAny<CancellationToken>())).ReturnsAsync(reports);

        // Act
        var result = await _service.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData((PolicyReportGroupType)(-1))]
    [InlineData((PolicyReportGroupType)4)]
    public async Task GetReportPoliciesAsync_InvalidGroupType_ReturnsValidationError(PolicyReportGroupType groupType)
    {
        // Arrange
        var filters = new PolicyReportFiltersDto(null, null, null, null, null);

        // Act
        var result = await _service.GetReportPoliciesAsync(groupType, filters, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PolicyReportErrors.InvalidGroupType.Code, result.Error?.Code);

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(It.IsAny<PolicyReportGroupType>(), It.IsAny<PolicyReportFiltersDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("2027-10-01", "2026-10-01")]
    [InlineData("2026-12-31", "2026-01-01")]
    public async Task GetReportPoliciesAsync_InvalidDateRange_ReturnsValidationError(string from, string to)
    {
        // Arrange
        DateTime? fromDate = from is null ? null : DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime? toDate = to is null ? null : DateTime.ParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var filters = new PolicyReportFiltersDto(fromDate, toDate, null, null, null);

        // Act
        var result = await _service.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PolicyReportErrors.InvalidDateRange.Code, result.Error?.Code);

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(It.IsAny<PolicyReportGroupType>(), It.IsAny<PolicyReportFiltersDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Unknown")]
    [InlineData("999")]
    public async Task GetReportPoliciesAsync_InvalidStatus_ReturnsValidationError(string status)
    {
        // Arrange
        var filters = new PolicyReportFiltersDto(null, null, status, null, null);

        // Act
        var result = await _service.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PolicyReportErrors.InvalidStatus.Code, result.Error?.Code);

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(It.IsAny<PolicyReportGroupType>(), It.IsAny<PolicyReportFiltersDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("currency")]
    [InlineData("buildingType")]
    public async Task GetReportPoliciesAsync_EmptyGuid_ReturnsValidationError(string field)
    {
        // Arrange
        string? currency = null;
        string? buildingType = null;

        if (field == "currency")
        {
            currency = Guid.Empty.ToString();
        }
        else
        {
            buildingType = Guid.Empty.ToString();
        }

        var filters = new PolicyReportFiltersDto(null, null, null, currency, buildingType);

        // Act
        var result = await _service.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);

        if (field == "currency")
        {
            Assert.Equal(PolicyReportErrors.InvalidCurrencyId.Code, result.Error?.Code);
        }
        else
        {
            Assert.Equal(PolicyReportErrors.InvalidBuildingTypeId.Code, result.Error?.Code);
        }

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(It.IsAny<PolicyReportGroupType>(), It.IsAny<PolicyReportFiltersDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetReportPoliciesAsync_BaseCurrencyNotConfigured_ReturnsFailure(string? baseCurrencyCode)
    {
        // Arrange
        _configurationMock.Setup(x => x["CurrencySettings:BaseCurrencyCode"]).Returns(baseCurrencyCode);

        var filters = new PolicyReportFiltersDto(null, null, null, null, null);

        // Act
        var result = await _service.GetReportPoliciesAsync(PolicyReportGroupType.Country, filters, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PolicyReportErrors.BaseCurrencyCodeNotConfigured.Code, result.Error?.Code);

        _repositoryMock.Verify(x => x.GetReportPoliciesAsync(It.IsAny<PolicyReportGroupType>(), It.IsAny<PolicyReportFiltersDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}