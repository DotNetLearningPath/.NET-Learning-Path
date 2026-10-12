using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Models.Services;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _repositoryMock;
    private readonly Mock<IPolicyReferenceService> _referenceServiceMock;
    private readonly Mock<IPolicyNumberGenerator> _numberGeneratorMock;
    private readonly Mock<IPolicyCalculationService> _calculationServiceMock;
    private readonly Mock<ILogger<PolicyService>> _loggerMock;
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _repositoryMock = new Mock<IPolicyRepository>();
        _referenceServiceMock = new Mock<IPolicyReferenceService>();
        _numberGeneratorMock = new Mock<IPolicyNumberGenerator>();
        _calculationServiceMock = new Mock<IPolicyCalculationService>();
        _loggerMock = new Mock<ILogger<PolicyService>>();

        _service = new PolicyService(
            _repositoryMock.Object,
            _referenceServiceMock.Object,
            _numberGeneratorMock.Object,
            _calculationServiceMock.Object,
            _loggerMock.Object);
    }

    #region Read Policy Tests

    [Fact]
    public async Task GetPolicyByIdAsync_ExistingPolicy_ReturnsSuccess()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();

        _repositoryMock.Setup(x => x.GetPolicyByIdAsync(policy.PolicyId, It.IsAny<CancellationToken>())).ReturnsAsync(policy);

        // Act
        var result = await _service.GetPolicyByIdAsync(policy.PolicyId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(policy.PolicyId, result.Value.PolicyId);
        Assert.Equal(policy.PolicyNumber, result.Value.PolicyNumber);
        Assert.Equal(policy.ClientId, result.Value.ClientId);
        Assert.Equal(policy.BuildingId, result.Value.BuildingId);
        Assert.Equal(policy.BrokerId, result.Value.BrokerId);
        Assert.Equal(policy.CurrencyId, result.Value.CurrencyId);
        Assert.Equal(policy.Status, result.Value.Status);
        Assert.Equal(policy.StartDate, result.Value.StartDate);
        Assert.Equal(policy.EndDate, result.Value.EndDate);
        Assert.Equal(policy.BasePremium, result.Value.BasePremium);
        Assert.Equal(policy.FinalPremium, result.Value.FinalPremium);
    }

    [Fact]
    public async Task GetPolicyByIdAsync_NonExistingPolicy_ReturnsNotFound()
    {
        // Arrange
        var policyId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetPolicyByIdAsync(policyId, It.IsAny<CancellationToken>())).ReturnsAsync((Policy?)null);

        // Act
        var result = await _service.GetPolicyByIdAsync(policyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(PolicyErrors.NotFound(policyId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetPolicyByIdAsync(policyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPolicyByIdAsync_EmptyPolicyId_ReturnsValidationError()
    {
        // Arrange
        var policyId = Guid.Empty;

        // Act
        var result = await _service.GetPolicyByIdAsync(policyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidPolicyId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetPolicyByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Search Policies Tests

    [Fact]
    public async Task SearchPoliciesAsync_ValidSearch_ReturnsPagedResult()
    {
        // Arrange
        var policy1 = TestData.CreateDraftPolicy1();
        var policy2 = TestData.CreateDraftPolicy2();
        var policies = new List<Policy> { policy1, policy2 };
        var search = new PolicySearchDto(null, null, null, null, null, 1, 20);

        _repositoryMock.Setup(x => x.SearchPoliciesAsync(search, It.IsAny<CancellationToken>())).ReturnsAsync((policies, policies.Count));

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(policies.Count, result.Value.Items.Count);
        Assert.Equal(policies.Count, result.Value.TotalCount);
        Assert.Equal(search.PageNumber, result.Value.PageNumber);
        Assert.Equal(search.PageSize, result.Value.PageSize);
        Assert.Equal(policy1.PolicyNumber, result.Value.Items[0].PolicyNumber);
        Assert.Equal(policy2.PolicyNumber, result.Value.Items[1].PolicyNumber);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(search, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchPoliciesAsync_InvalidPageNumber_ReturnsValidationError(int pageNumber)
    {
        // Arrange
        var search = new PolicySearchDto(null, null, null, null, null, pageNumber, 20);

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidPageNumber.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(It.IsAny<PolicySearchDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task SearchPoliciesAsync_InvalidPageSize_ReturnsValidationError(int pageSize)
    {
        // Arrange
        var search = new PolicySearchDto(null, null, null, null, null, 1, pageSize);

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidPageSize.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(It.IsAny<PolicySearchDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchPoliciesAsync_InvalidStatus_ReturnsValidationError()
    {
        // Arrange
        var search = new PolicySearchDto(null, null, (PolicyStatus)999, null, null, 1, 20);

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidStatus.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(It.IsAny<PolicySearchDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchPoliciesAsync_InvalidDateRange_ReturnsValidationError()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(10);
        var endDate = startDate.AddDays(-1);
        var search = new PolicySearchDto(null, null, null, startDate, endDate, 1, 20);

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidDateRange.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(It.IsAny<PolicySearchDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("client")]
    [InlineData("broker")]
    public async Task SearchPoliciesAsync_EmptyFilterId_ReturnsValidationError(string field)
    {
        // Arrange
        var search = new PolicySearchDto(null, null, null, null, null, 1, 20);

        Error expectedError;

        if (field == "client")
        {
            expectedError = PolicyErrors.InvalidClientId;
            search = search with { ClientId = Guid.Empty };
        }
        else
        {
            expectedError = PolicyErrors.InvalidBrokerId;
            search = search with { BrokerId = Guid.Empty };
        }

        // Act
        var result = await _service.SearchPoliciesAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchPoliciesAsync(It.IsAny<PolicySearchDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Create Policy Tests

    [Fact]
    public async Task CreatePolicyAsync_ValidPolicy_ReturnsSuccess()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();
        var references = TestData.CreatePolicyReferences(policyDto);
        var expectedFinalPremium = TestData.PolicyFinalPremium;

        SetupValidPolicyReferences(policyDto, references);
        SetupPolicyNumberDoesNotExist();
        SetupCalculatedPremium(policyDto, references.Building, expectedFinalPremium);

        // Act
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TestData.PolicyNumber, result.Value.PolicyNumber);
        Assert.Equal(policyDto.ClientId, result.Value.ClientId);
        Assert.Equal(policyDto.BuildingId, result.Value.BuildingId);
        Assert.Equal(policyDto.BrokerId, result.Value.BrokerId);
        Assert.Equal(policyDto.CurrencyId, result.Value.CurrencyId);
        Assert.Equal(PolicyStatus.Draft, result.Value.Status);
        Assert.Equal(policyDto.StartDate, result.Value.StartDate);
        Assert.Equal(policyDto.EndDate, result.Value.EndDate);
        Assert.Equal(policyDto.BasePremium, result.Value.BasePremium);
        Assert.Equal(expectedFinalPremium, result.Value.FinalPremium);

        _repositoryMock.Verify(x => x.PolicyNumberExistsAsync(TestData.PolicyNumber, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddPolicyAsync(It.Is<Policy>(policy => policy.PolicyNumber == TestData.PolicyNumber && policy.Status == PolicyStatus.Draft && policy.FinalPremium == expectedFinalPremium), It.IsAny<CancellationToken>()), Times.Once);
        _calculationServiceMock.Verify(x => x.CalculateFinalPremiumAsync(policyDto.BasePremium, references.Building, policyDto.StartDate, It.IsAny<CancellationToken>()), Times.Once);
        _numberGeneratorMock.Verify(x => x.GeneratePolicyNumber(), Times.Once);
    }

    [Theory]
    [InlineData("client")]
    [InlineData("building")]
    [InlineData("broker")]
    [InlineData("currency")]
    public async Task CreatePolicyAsync_EmptyReferenceId_ReturnsValidationError(string field)
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();

        var expectedError = field switch
        {
            "client" => PolicyErrors.InvalidClientId,
            "building" => PolicyErrors.InvalidBuildingId,
            "broker" => PolicyErrors.InvalidBrokerId,
            _ => PolicyErrors.InvalidCurrencyId
        };

        policyDto = field switch
        {
            "client" => policyDto with { ClientId = Guid.Empty },
            "building" => policyDto with { BuildingId = Guid.Empty },
            "broker" => policyDto with { BrokerId = Guid.Empty },
            _ => policyDto with { CurrencyId = Guid.Empty }
        };

        // Act & Assert
        await AssertInvalidCreatePolicyAsync(policyDto, expectedError);
    }

    [Fact]
    public async Task CreatePolicyAsync_InvalidDateRange_ReturnsValidationError()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto() with { EndDate = DateTime.UtcNow.Date.AddDays(-1) };

        // Act & Assert
        await AssertInvalidCreatePolicyAsync(policyDto, PolicyErrors.InvalidDateRange);
    }

    [Fact]
    public async Task CreatePolicyAsync_InvalidPolicyReferences_ReturnsError()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();

        _referenceServiceMock.Setup(x => x.ValidatePolicyReferencesAsync(policyDto.ClientId, policyDto.BuildingId, policyDto.BrokerId, policyDto.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<PolicyReferences>.Failure(PolicyErrors.InactiveBroker));

        // Act
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.InactiveBroker.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.InactiveBroker.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePolicyAsync_DuplicatePolicyNumber_ReturnsError()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidPolicyReferences(policyDto, references);
        _numberGeneratorMock.Setup(x => x.GeneratePolicyNumber()).Returns(TestData.PolicyNumber);

        _repositoryMock.Setup(x => x.PolicyNumberExistsAsync(TestData.PolicyNumber, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.PolicyNumberNotGenerated.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.PolicyNumberNotGenerated.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePolicyAsync_CalculationFailed_ReturnsError()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidPolicyReferences(policyDto, references);
        SetupPolicyNumberDoesNotExist();

        _calculationServiceMock.Setup(x => x.CalculateFinalPremiumAsync(policyDto.BasePremium, references.Building, policyDto.StartDate, It.IsAny<CancellationToken>())).ReturnsAsync(Result<decimal>.Failure(PolicyErrors.InvalidBasePremium));

        // Act
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.InvalidBasePremium.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidBasePremium.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePolicyAsync_DuplicateOnInsertConcurrency_ReturnsError()
    {
        // Arrange
        var policyDto = TestData.CreatePolicyDto();
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidPolicyReferences(policyDto, references);
        SetupPolicyNumberDoesNotExist();
        SetupCalculatedPremium(policyDto, references.Building, TestData.PolicyFinalPremium);

        _repositoryMock.Setup(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Policy)));

        // Act
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.PolicyNumberNotGenerated.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.PolicyNumberNotGenerated.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Activate Policy Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ActivatePolicyAsync_ValidStartDate_ReturnsActivatedPolicy(int daysFromToday)
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.StartDate = DateTime.UtcNow.Date.AddDays(daysFromToday);

        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupExistingPolicyForUpdate(policy);
        SetupValidPolicyReferences(policyDto, references);

        // Act
        var result = await _service.ActivatePolicyAsync(policy.PolicyId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(policy.PolicyId, result.Value.PolicyId);
        Assert.Equal(PolicyStatus.Active, result.Value.Status);
        Assert.NotNull(policy.ActivationDate);
        Assert.NotNull(policy.ModifiedAt);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivatePolicyAsync_EmptyPolicyId_ReturnsValidationError()
    {
        // Arrange
        var policyId = Guid.Empty;

        // Act
        var result = await _service.ActivatePolicyAsync(policyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidPolicyId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetPolicyForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivatePolicyAsync_NonExistingPolicy_ReturnsNotFound()
    {
        // Arrange
        var policyId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetPolicyForUpdateAsync(policyId, It.IsAny<CancellationToken>())).ReturnsAsync((Policy?)null);

        // Act
        var result = await _service.ActivatePolicyAsync(policyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(PolicyErrors.NotFound(policyId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivatePolicyAsync_PolicyNotDraft_ReturnsValidationError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.Status = PolicyStatus.Active;

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.ActivatePolicyAsync(policy.PolicyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.PolicyNotDraft.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivatePolicyAsync_StartDateInPast_ReturnsValidationError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.StartDate = DateTime.UtcNow.Date.AddDays(-1);

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.ActivatePolicyAsync(policy.PolicyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.StartDateInPast.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivatePolicyAsync_InvalidPolicyReferences_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();

        SetupExistingPolicyForUpdate(policy);

        _referenceServiceMock.Setup(x => x.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<PolicyReferences>.Failure(PolicyErrors.InactiveBroker));

        // Act
        var result = await _service.ActivatePolicyAsync(policy.PolicyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.InactiveBroker.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.InactiveBroker.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Cancel Policy Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task CancelPolicyAsync_ValidEndDate_ReturnsCancelledPolicy(int daysFromToday)
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.Status = PolicyStatus.Active;
        policy.EndDate = DateTime.UtcNow.Date.AddDays(daysFromToday);

        var cancelPolicyDto = TestData.CreateCancelPolicyDto();

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.CancelPolicyAsync(policy.PolicyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(policy.PolicyId, result.Value.PolicyId);
        Assert.Equal(PolicyStatus.Cancelled, result.Value.Status);
        Assert.NotNull(policy.CancellationDate);
        Assert.Equal(cancelPolicyDto.CancellationReason, policy.CancellationReason);
        Assert.NotNull(policy.ModifiedAt);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelPolicyAsync_EmptyPolicyId_ReturnsValidationError()
    {
        // Arrange
        var policyId = Guid.Empty;
        var cancelPolicyDto = TestData.CreateCancelPolicyDto();

        // Act
        var result = await _service.CancelPolicyAsync(policyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.InvalidPolicyId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetPolicyForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelPolicyAsync_CancellationReasonMaxLength_ReturnsSuccess()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.Status = PolicyStatus.Active;

        var cancelPolicyDto = TestData.CreateCancelPolicyDto() with
        {
            CancellationReason = new string('A', PolicyConstraints.CancellationReasonMaxLength)
        };

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.CancelPolicyAsync(policy.PolicyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(PolicyStatus.Cancelled, result.Value.Status);
        Assert.Equal(cancelPolicyDto.CancellationReason, policy.CancellationReason);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task CancelPolicyAsync_InvalidCancellationReason_ReturnsValidationError(string reason)
    {
        // Arrange
        var cancelPolicyDto = TestData.CreateCancelPolicyDto() with { CancellationReason = reason };

        // Act & Assert
        await AssertInvalidCancelPolicyAsync(cancelPolicyDto, PolicyErrors.CancellationReasonRequired);
    }

    [Fact]
    public async Task CancelPolicyAsync_CancellationReasonTooLong_ReturnsValidationError()
    {
        // Arrange
        var cancelPolicyDto = TestData.CreateCancelPolicyDto() with
        {
            CancellationReason = new string('A', PolicyConstraints.CancellationReasonMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCancelPolicyAsync(cancelPolicyDto, PolicyErrors.InvalidCancellationReasonMaxLength);
    }

    [Fact]
    public async Task CancelPolicyAsync_PolicyNotActive_ReturnsValidationError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var cancelPolicyDto = TestData.CreateCancelPolicyDto();

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.CancelPolicyAsync(policy.PolicyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(PolicyErrors.CancellationInactivePolicy.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelPolicyAsync_EndDateYesterday_ReturnsValidationError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        policy.Status = PolicyStatus.Active;
        policy.EndDate = DateTime.UtcNow.Date.AddDays(-1);
        var cancelPolicyDto = TestData.CreateCancelPolicyDto();

        SetupExistingPolicyForUpdate(policy);

        // Act
        var result = await _service.CancelPolicyAsync(policy.PolicyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.CancellationExpiredPolicy.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.CancellationExpiredPolicy.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelPolicyAsync_NonExistingPolicy_ReturnsNotFound()
    {
        // Arrange
        var policyId = TestData.NonExistingId;
        var cancelPolicyDto = TestData.CreateCancelPolicyDto();

        _repositoryMock.Setup(x => x.GetPolicyForUpdateAsync(policyId, It.IsAny<CancellationToken>())).ReturnsAsync((Policy?)null);

        // Act
        var result = await _service.CancelPolicyAsync(policyId, cancelPolicyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(PolicyErrors.NotFound(policyId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Helpers

    private void SetupExistingPolicyForUpdate(Policy policy)
    {
        _repositoryMock.Setup(x => x.GetPolicyForUpdateAsync(policy.PolicyId, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
    }

    private void SetupValidPolicyReferences(CreatePolicyDto policyDto, PolicyReferences references)
    {
        _referenceServiceMock.Setup(x => x.ValidatePolicyReferencesAsync(policyDto.ClientId, policyDto.BuildingId, policyDto.BrokerId, policyDto.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<PolicyReferences>.Success(references));
    }

    private void SetupPolicyNumberDoesNotExist()
    {
        _numberGeneratorMock.Setup(x => x.GeneratePolicyNumber()).Returns(TestData.PolicyNumber);
        _repositoryMock.Setup(x => x.PolicyNumberExistsAsync(TestData.PolicyNumber, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupCalculatedPremium(CreatePolicyDto policyDto, Building building, decimal finalPremium)
    {
        _calculationServiceMock.Setup(x => x.CalculateFinalPremiumAsync(policyDto.BasePremium, building, policyDto.StartDate, It.IsAny<CancellationToken>())).ReturnsAsync(Result<decimal>.Success(finalPremium));
    }

    private async Task AssertInvalidCreatePolicyAsync(CreatePolicyDto policyDto, Error expectedError)
    {
        var result = await _service.CreatePolicyAsync(policyDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _referenceServiceMock.Verify(x => x.ValidatePolicyReferencesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidCancelPolicyAsync(CancelPolicyDto cancelPolicyDto, Error expectedError)
    {
        var policy = TestData.CreateDraftPolicy1();

        var result = await _service.CancelPolicyAsync(policy.PolicyId, cancelPolicyDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetPolicyForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SavePolicyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion
}