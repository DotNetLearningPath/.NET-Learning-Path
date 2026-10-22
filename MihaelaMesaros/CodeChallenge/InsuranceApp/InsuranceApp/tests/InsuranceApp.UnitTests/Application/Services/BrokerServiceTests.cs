using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Broker;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class BrokerServiceTests
{
    private readonly Mock<IBrokerRepository> _repositoryMock;
    private readonly Mock<ILogger<BrokerService>> _loggerMock;
    private readonly BrokerService _service;

    public BrokerServiceTests()
    {
        _repositoryMock = new Mock<IBrokerRepository>();
        _loggerMock = new Mock<ILogger<BrokerService>>();

        _service = new BrokerService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Read Broker Tests

    [Fact]
    public async Task GetBrokersAsync_ReturnsBrokers()
    {
        // Arrange
        var broker1 = TestData.CreateBroker1();
        var broker2 = TestData.CreateBroker2();
        var brokers = new List<Broker> { broker1, broker2 };

        _repositoryMock.Setup(x => x.GetBrokersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(brokers);

        // Act
        var result = await _service.GetBrokersAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(brokers.Count, result.Value.Count);
        Assert.Equal(broker1.BrokerCode, result.Value[0].BrokerCode);
        Assert.Equal(broker2.BrokerCode, result.Value[1].BrokerCode);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_ExistingBroker_ReturnsSuccess()
    {
        // Arrange
        var broker = TestData.CreateBroker1();
        SetupExistingBrokerById(broker);

        // Act
        var result = await _service.GetBrokerByIdAsync(broker.BrokerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(broker.BrokerId, result.Value.BrokerId);
        Assert.Equal(broker.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(broker.Name, result.Value.Name);
        Assert.Equal(broker.Email, result.Value.Email);
        Assert.Equal(broker.Phone, result.Value.Phone);
        Assert.Equal(broker.CommissionPercentage, result.Value.CommissionPercentage);
        Assert.Equal(broker.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetBrokerByIdAsync(brokerId, It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        // Act
        var result = await _service.GetBrokerByIdAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BrokerErrors.NotFound(brokerId).Code, result.Error.Code);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Arrange
        var brokerId = Guid.Empty;

        // Act
        var result = await _service.GetBrokerByIdAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BrokerErrors.InvalidBrokerId.Code,result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerByIdAsync(It.IsAny<Guid>(),It.IsAny<CancellationToken>()),Times.Never);
    }

    #endregion

    #region Create Broker Tests

    [Fact]
    public async Task CreateBrokerAsync_ValidBroker_ReturnsSuccess()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        SetupBrokerCodeDoesNotExistForCreate(createBrokerDto);

        // Act
        var result = await _service.CreateBrokerAsync(createBrokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(createBrokerDto.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(createBrokerDto.Name, result.Value.Name);
        Assert.Equal(createBrokerDto.Email, result.Value.Email);
        Assert.Equal(createBrokerDto.Phone, result.Value.Phone);
        Assert.Equal(createBrokerDto.CommissionPercentage,result.Value.CommissionPercentage);
        Assert.Equal(createBrokerDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(x => x.BrokerCodeExistsAsync(createBrokerDto.BrokerCode, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(
            x => x.AddBrokerAsync(
                It.Is<Broker>(broker =>
                    broker.BrokerCode == createBrokerDto.BrokerCode &&
                    broker.Name == createBrokerDto.Name &&
                    broker.Email == createBrokerDto.Email &&
                    broker.Phone == createBrokerDto.Phone &&
                    broker.CommissionPercentage == createBrokerDto.CommissionPercentage &&
                    broker.IsActive == createBrokerDto.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBrokerAsync_ValidBroker_NormalizesValues()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            BrokerCode = $" {createBrokerDto.BrokerCode.ToLowerInvariant()} ",
            Name = $" {createBrokerDto.Name} ",
            Email = $" {createBrokerDto.Email} ",
            Phone = $" {createBrokerDto.Phone} "
        };

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(createBrokerDto.BrokerCode, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBrokerAsync(brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(createBrokerDto.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(createBrokerDto.Name, result.Value.Name);
        Assert.Equal(createBrokerDto.Email, result.Value.Email);
        Assert.Equal(createBrokerDto.Phone, result.Value.Phone);

        _repositoryMock.Verify(
            x => x.AddBrokerAsync(
                It.Is<Broker>(b =>
                    b.BrokerCode == createBrokerDto.BrokerCode &&
                    b.Name == createBrokerDto.Name &&
                    b.Email == createBrokerDto.Email &&
                    b.Phone == createBrokerDto.Phone),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBrokerAsync_MissingBrokerCode_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            BrokerCode = ""
        };
        
        // Act & Assert 
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.BrokerCodeRequired);
    }

    [Fact]
    public async Task CreateBrokerAsync_BrokerCodeTooShort_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMinLength - 1)
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_BrokerCodeTooLong_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Name = ""
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.NameRequired);
    }

    [Fact]
    public async Task CreateBrokerAsync_NameTooShort_ReturnsValidationError()
    {
        // Arrange 
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Name = new string('A', BrokerConstraints.NameMinLength - 1)
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Name = new string( 'A', BrokerConstraints.NameMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidNameLength);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData(TestData.EmailDomain)]
    public async Task CreateBrokerAsync_InvalidEmail_ReturnsValidationError(string email)
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Email = email
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateBrokerAsync_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var email = $"{new string('a', BrokerConstraints.EmailMaxLength - TestData.EmailDomain.Length + 1)}{TestData.EmailDomain}";

        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Email = email
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateBrokerAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Phone = new string('1', BrokerConstraints.PhoneMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidPhoneLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            CommissionPercentage =  BrokerConstraints.MinCommissionPercentage - 0.01m
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            CommissionPercentage = BrokerConstraints.MaxCommissionPercentage + 0.01m
        };

        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            CommissionPercentage = TestData.InvalidCommissionPercentageScale
        };
        
        // Act & Assert
        await AssertInvalidCreateBrokerAsync(brokerDto, BrokerErrors.InvalidCommissionPercentageScale);
    }

    [Fact]
    public async Task CreateBrokerAsync_NullCommissionPercentage_ReturnsSuccess()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            CommissionPercentage = null
        };

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(brokerDto.BrokerCode,null,It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBrokerAsync(brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.CommissionPercentage);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task CreateBrokerAsync_OptionalContactInfo_ReturnsSuccess(bool emailIsNull, bool phoneIsNull)
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        var brokerDto = createBrokerDto with
        {
            Email = emailIsNull ? null : createBrokerDto.Email,
            Phone = phoneIsNull ? null : createBrokerDto.Phone
        };
        SetupBrokerCodeDoesNotExistForCreate(brokerDto);
        
        // Act
        var result = await _service.CreateBrokerAsync(brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(brokerDto.Email, result.Value.Email);
        Assert.Equal(brokerDto.Phone, result.Value.Phone);
    }

    [Fact]
    public async Task CreateBrokerAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(createBrokerDto.BrokerCode,null,It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateBrokerAsync(createBrokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(createBrokerDto.BrokerCode).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddBrokerAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBrokerAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        var createBrokerDto = TestData.CreateBrokerDto();
        SetupBrokerCodeDoesNotExistForCreate(createBrokerDto);

        _repositoryMock .Setup(x => x.AddBrokerAsync(It.IsAny<Broker>(),It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Broker)));

        // Act
        var result = await _service.CreateBrokerAsync(createBrokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(createBrokerDto.BrokerCode).Code, result.Error.Code);
    }

    #endregion

    #region Update Broker Tests

    [Fact]
    public async Task UpdateBrokerAsync_ValidBroker_ReturnsUpdatedBroker()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerDto = TestData.UpdateBrokerDto();

        SetupExistingBrokerForUpdate(brokerToUpdate);
        SetupBrokerCodeDoesNotExistForUpdate(brokerDto, brokerToUpdate);

        // Act
        var result = await _service.UpdateBrokerAsync(brokerToUpdate.BrokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(brokerDto.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(brokerDto.Name, result.Value.Name);
        Assert.Equal(brokerDto.Email, result.Value.Email);
        Assert.Equal(brokerDto.Phone, result.Value.Phone);
        Assert.Equal(brokerDto.CommissionPercentage, result.Value.CommissionPercentage);
        Assert.NotNull(brokerToUpdate.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Arrange
        var brokerId = Guid.Empty;
        var brokerDto = TestData.UpdateBrokerDto();

        // Act
        var result = await _service.UpdateBrokerAsync(brokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BrokerErrors.InvalidBrokerId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("BR")]
    public async Task UpdateBrokerAsync_InvalidBrokerCode_ReturnsValidationError(string brokerCode)
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with { BrokerCode = brokerCode };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, brokerCode == "" ? BrokerErrors.BrokerCodeRequired : BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task UpdateBrokerAsync_BrokerCodeTooLong_ReturnsValidationError()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task UpdateBrokerAsync_InvalidName_ReturnsValidationError()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with
        {
            Name = new string('A', BrokerConstraints.NameMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidNameLength);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData(TestData.EmailDomain)]
    public async Task UpdateBrokerAsync_InvalidEmail_ReturnsValidationError(string email)
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with { Email = email };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task UpdateBrokerAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with
        {
            Phone = new string('1', BrokerConstraints.PhoneMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidPhoneLength);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public async Task UpdateBrokerAsync_CommissionPercentageOutsideRange_ReturnsValidationError(decimal percentage)
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with { CommissionPercentage = percentage };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task UpdateBrokerAsync_CommissionPercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerUpdateDto = TestData.UpdateBrokerDto();
        var brokerDto = brokerUpdateDto with { CommissionPercentage = TestData.InvalidCommissionPercentageScale };

        // Act & Assert
        await AssertInvalidUpdateBrokerAsync(brokerDto, brokerToUpdate, BrokerErrors.InvalidCommissionPercentageScale);
    }

    [Fact]
    public async Task UpdateBrokerAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetBrokerForUpdateAsync(brokerId, It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        var brokerDto = TestData.UpdateBrokerDto();

        // Act
        var result = await _service.UpdateBrokerAsync(brokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BrokerErrors.NotFound(brokerId).Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateBrokerAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var existingBroker = TestData.CreateBroker2();

        var brokerDto = TestData.UpdateBrokerDto() with
        {
            BrokerCode = existingBroker.BrokerCode
        };

        SetupExistingBrokerForUpdate(brokerToUpdate);

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(existingBroker.BrokerCode, brokerToUpdate.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateBrokerAsync(brokerToUpdate.BrokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(brokerDto.BrokerCode).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBrokerAsync_SameCode_ExcludesCurrentBroker()
    {
        // Arrange
        var existingBroker = TestData.CreateBroker1();
        var brokerDto = TestData.UpdateBrokerDto();

        SetupExistingBrokerForUpdate(existingBroker);
        SetupBrokerCodeDoesNotExistForUpdate(brokerDto, existingBroker);

        // Act
        var result = await _service.UpdateBrokerAsync(existingBroker.BrokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.BrokerCodeExistsAsync(brokerDto.BrokerCode, existingBroker.BrokerId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBrokerAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        var brokerToUpdate = TestData.CreateBroker1();
        var brokerDto = TestData.UpdateBrokerDto();

        SetupExistingBrokerForUpdate(brokerToUpdate);
        SetupBrokerCodeDoesNotExistForUpdate(brokerDto, brokerToUpdate);

        _repositoryMock.Setup(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Broker)));

        // Act
        var result = await _service.UpdateBrokerAsync(brokerToUpdate.BrokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(brokerDto.BrokerCode).Code, result.Error.Code);
    }

    #endregion

    #region Activate Broker Tests

    [Fact]
    public async Task ActivateBrokerAsync_ExistingBroker_ReturnsActivatedBroker()
    {
        // Arrange
        var existingBroker = TestData.CreateBroker1();
        existingBroker.IsActive = false;

        SetupExistingBrokerForUpdate(existingBroker);

        // Act
        var result = await _service.ActivateBrokerAsync(existingBroker.BrokerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.IsActive);
        Assert.NotNull(existingBroker.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Arrange
        var brokerId = Guid.Empty;

        // Act
        var result = await _service.ActivateBrokerAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BrokerErrors.InvalidBrokerId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(),It.IsAny<CancellationToken>()),Times.Never);
    }

    [Fact]
    public async Task ActivateBrokerAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetBrokerForUpdateAsync(brokerId,It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        // Act
        var result = await _service.ActivateBrokerAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BrokerErrors.NotFound(brokerId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()),Times.Never);
    }

    #endregion

    #region Deactivate Broker Tests

    [Fact]
    public async Task DeactivateBrokerAsync_ExistingBroker_ReturnsDeactivatedBroker()
    {
        // Arrange
        var existingBroker = TestData.CreateBroker1();
        existingBroker.IsActive = true;

        SetupExistingBrokerForUpdate(existingBroker);

        // Act
        var result = await _service.DeactivateBrokerAsync(existingBroker.BrokerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value.IsActive);
        Assert.NotNull(existingBroker.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Arrange
        var brokerId = Guid.Empty;

        // Act
        var result = await _service.DeactivateBrokerAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BrokerErrors.InvalidBrokerId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateBrokerAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetBrokerForUpdateAsync(brokerId, It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        // Act
        var result = await _service.DeactivateBrokerAsync(brokerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BrokerErrors.NotFound(brokerId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion


    #region Helpers

    private void SetupExistingBrokerById(Broker broker)
    {
        _repositoryMock.Setup(x => x.GetBrokerByIdAsync(broker.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
    }

    private void SetupExistingBrokerForUpdate(Broker broker)
    {
        _repositoryMock.Setup(x => x.GetBrokerForUpdateAsync(broker.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(broker);
    }

    private void SetupBrokerCodeDoesNotExistForCreate(CreateBrokerDto brokerDto)
    {
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(brokerDto.BrokerCode, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupBrokerCodeDoesNotExistForUpdate(UpdateBrokerDto updateBrokerDto, Broker broker)
    {
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(updateBrokerDto.BrokerCode, broker.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private async Task AssertInvalidCreateBrokerAsync(CreateBrokerDto brokerDto, Error expectedError)
    {
        var result = await _service.CreateBrokerAsync(brokerDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddBrokerAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidUpdateBrokerAsync(UpdateBrokerDto brokerDto, Broker broker, Error expectedError)
    {
        var result = await _service.UpdateBrokerAsync(broker.BrokerId, brokerDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
