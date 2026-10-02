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

    private readonly CreateBrokerDto _validCreateBrokerDto;
    private readonly UpdateBrokerDto _validUpdateBrokerDto;
    private readonly Broker _existingBroker1, _existingBroker2;

    public BrokerServiceTests()
    {
        _repositoryMock = new Mock<IBrokerRepository>();
        _loggerMock = new Mock<ILogger<BrokerService>>();

        _service = new BrokerService(
            _repositoryMock.Object,
            _loggerMock.Object);


        _validCreateBrokerDto = new CreateBrokerDto(
            "BR001",
            "Test Broker",
            "broker@test.com",
            "0712345678",
            5.25m,
            true);

        _validUpdateBrokerDto = new UpdateBrokerDto(
            "BR001",
            "Test Broker Updated",
            "updated.broker@test.com",
            "0722345678",
            3.50m);

        _existingBroker1 = new Broker
        {
            BrokerId = Guid.NewGuid(),
            BrokerCode = "BR001",
            Name = "Test Broker 01",
            Email = "broker01@test.com",
            Phone = "0712345678",
            CommissionPercentage = 5.25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _existingBroker2 = new Broker
        {
            BrokerId = Guid.NewGuid(),
            BrokerCode = "BR002",
            Name = "Test Broker 02",
            Email = "broker02@test.com",
            Phone = "0712345678",
            CommissionPercentage = 4.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    }

    #region Read Broker Tests

    [Fact]
    public async Task GetBrokersAsync_ReturnsBrokers()
    {
        // Arrange
        var brokers = new List<Broker> { _existingBroker1, _existingBroker2 };

        _repositoryMock.Setup(x => x.GetBrokersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(brokers);

        // Act
        var result = await _service.GetBrokersAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(_existingBroker1.BrokerCode, result.Value[0].BrokerCode);
        Assert.Equal(_existingBroker2.BrokerCode, result.Value[1].BrokerCode);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_ExistingBroker_ReturnsSuccess()
    {
        // Arrange
        SetupExistingBrokerById(_existingBroker1);

        // Act
        var result = await _service.GetBrokerByIdAsync(_existingBroker1.BrokerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_existingBroker1.BrokerId, result.Value.BrokerId);
        Assert.Equal(_existingBroker1.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(_existingBroker1.Name, result.Value.Name);
        Assert.Equal(_existingBroker1.Email, result.Value.Email);
        Assert.Equal(_existingBroker1.Phone, result.Value.Phone);
        Assert.Equal(_existingBroker1.CommissionPercentage, result.Value.CommissionPercentage);
        Assert.Equal(_existingBroker1.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestConstants.NonExistingId;

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
        // Act
        var result = await _service.GetBrokerByIdAsync(Guid.Empty, CancellationToken.None);

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
        SetupBrokerCodeDoesNotExistForCreate();

        // Act
        var result = await _service.CreateBrokerAsync(_validCreateBrokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_validCreateBrokerDto.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(_validCreateBrokerDto.Name, result.Value.Name);
        Assert.Equal(_validCreateBrokerDto.Email, result.Value.Email);
        Assert.Equal(_validCreateBrokerDto.Phone, result.Value.Phone);
        Assert.Equal(_validCreateBrokerDto.CommissionPercentage,result.Value.CommissionPercentage);
        Assert.Equal(_validCreateBrokerDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddBrokerAsync(
                It.Is<Broker>(broker =>
                    broker.BrokerCode == _validCreateBrokerDto.BrokerCode &&
                    broker.Name == _validCreateBrokerDto.Name &&
                    broker.Email == _validCreateBrokerDto.Email &&
                    broker.Phone == _validCreateBrokerDto.Phone &&
                    broker.CommissionPercentage == _validCreateBrokerDto.CommissionPercentage &&
                    broker.IsActive == _validCreateBrokerDto.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBrokerAsync_ValidBroker_NormalizesValues()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            BrokerCode = " br001 ",
            Name = " Test Broker ",
            Email = " broker@test.com ",
            Phone = " 0712345678 "
        };

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync("BR001",null,It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBrokerAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("BR001", result.Value.BrokerCode);
        Assert.Equal("Test Broker", result.Value.Name);
        Assert.Equal("broker@test.com", result.Value.Email);
        Assert.Equal("0712345678", result.Value.Phone);

        _repositoryMock.Verify(
            x => x.AddBrokerAsync(
                It.Is<Broker>(broker =>
                    broker.BrokerCode == "BR001" &&
                    broker.Name == "Test Broker" &&
                    broker.Email == "broker@test.com" &&
                    broker.Phone == "0712345678"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBrokerAsync_MissingBrokerCode_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            BrokerCode = ""
        };
        
        // Act & Assert 
        await AssertInvalidCreateAsync(dto, BrokerErrors.BrokerCodeRequired);
    }

    [Fact]
    public async Task CreateBrokerAsync_BrokerCodeTooShort_ReturnsValidationError()
    {
        //Arrange
        var dto = _validCreateBrokerDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMinLength - 1)
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_BrokerCodeTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            Name = ""
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.NameRequired);
    }

    [Fact]
    public async Task CreateBrokerAsync_NameTooShort_ReturnsValidationError()
    {
        // Arrange 
        var dto = _validCreateBrokerDto with
        {
            Name = new string('A', BrokerConstraints.NameMinLength - 1)
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            Name = new string( 'A', BrokerConstraints.NameMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidNameLength);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@test.com")]
    public async Task CreateBrokerAsync_InvalidEmail_ReturnsValidationError(string email)
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            Email = email
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateBrokerAsync_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var email = $"{new string('a', BrokerConstraints.EmailMaxLength - "@test.com".Length + 1)}@test.com";

        var dto = _validCreateBrokerDto with
        {
            Email = email
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateBrokerAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            Phone = new string('1', BrokerConstraints.PhoneMaxLength + 1)
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidPhoneLength);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            CommissionPercentage =  BrokerConstraints.MinCommissionPercentage - 0.01m
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            CommissionPercentage = BrokerConstraints.MaxCommissionPercentage + 0.01m
        };

        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task CreateBrokerAsync_CommissionPercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            CommissionPercentage = 5.123m
        };
        
        // Act & Assert
        await AssertInvalidCreateAsync(dto, BrokerErrors.InvalidCommissionPercentageScale);
    }

    [Fact]
    public async Task CreateBrokerAsync_NullCommissionPercentage_ReturnsSuccess()
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            CommissionPercentage = null
        };

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(dto.BrokerCode,null,It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBrokerAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.CommissionPercentage);
    }

    [Theory]
    [InlineData(null, "0712345678")]
    [InlineData("broker@test.com", null)]
    [InlineData(null, null)]
    public async Task CreateBrokerAsync_OptionalContactInfo_ReturnsSuccess(string? email, string? phone)
    {
        // Arrange
        var dto = _validCreateBrokerDto with
        {
            Email = email,
            Phone = phone
        };

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(dto.BrokerCode,null,It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBrokerAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(phone, result.Value.Phone);
    }

    [Fact]
    public async Task CreateBrokerAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(_validCreateBrokerDto.BrokerCode,null,It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateBrokerAsync(_validCreateBrokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(_validCreateBrokerDto.BrokerCode).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddBrokerAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBrokerAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        SetupBrokerCodeDoesNotExistForCreate();

        _repositoryMock .Setup(x => x.AddBrokerAsync(It.IsAny<Broker>(),It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Broker)));

        // Act
        var result = await _service.CreateBrokerAsync(_validCreateBrokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(_validCreateBrokerDto.BrokerCode).Code, result.Error.Code);
    }

    #endregion

    #region Update Broker Tests

    [Fact]
    public async Task UpdateBrokerAsync_ValidBroker_ReturnsUpdatedBroker()
    {
        // Arrange
        var brokerDto = _validUpdateBrokerDto with
        {
            BrokerCode = "BR002",
            Name = "Updated Broker",
            Email = "updated@test.com",
            Phone = "0722222222",
            CommissionPercentage = 7.50m
        };

        SetupExistingBrokerForUpdate(_existingBroker1);

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(brokerDto.BrokerCode, _existingBroker1.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateBrokerAsync(_existingBroker1.BrokerId, brokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(brokerDto.BrokerCode, result.Value.BrokerCode);
        Assert.Equal(brokerDto.Name, result.Value.Name);
        Assert.Equal(brokerDto.Email, result.Value.Email);
        Assert.Equal(brokerDto.Phone, result.Value.Phone);
        Assert.Equal(brokerDto.CommissionPercentage, result.Value.CommissionPercentage);
        Assert.NotNull(_existingBroker1.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateBrokerAsync(Guid.Empty, _validUpdateBrokerDto, CancellationToken.None);

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
        var dto = _validUpdateBrokerDto with { BrokerCode = brokerCode };

        await AssertInvalidUpdateAsync(dto, brokerCode == "" ? BrokerErrors.BrokerCodeRequired : BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task UpdateBrokerAsync_BrokerCodeTooLong_ReturnsValidationError()
    {
        var dto = _validUpdateBrokerDto with
        {
            BrokerCode = new string('B', BrokerConstraints.BrokerCodeMaxLength + 1)
        };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidBrokerCodeLength);
    }

    [Fact]
    public async Task UpdateBrokerAsync_InvalidName_ReturnsValidationError()
    {
        var dto = _validUpdateBrokerDto with
        {
            Name = new string('A', BrokerConstraints.NameMaxLength + 1)
        };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidNameLength);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@test.com")]
    public async Task UpdateBrokerAsync_InvalidEmail_ReturnsValidationError(string email)
    {
        var dto = _validUpdateBrokerDto with { Email = email };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidEmail);
    }

    [Fact]
    public async Task UpdateBrokerAsync_PhoneTooLong_ReturnsValidationError()
    {
        var dto = _validUpdateBrokerDto with
        {
            Phone = new string('1', BrokerConstraints.PhoneMaxLength + 1)
        };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidPhoneLength);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public async Task UpdateBrokerAsync_CommissionPercentageOutsideRange_ReturnsValidationError(decimal percentage)
    {
        var dto = _validUpdateBrokerDto with { CommissionPercentage = percentage };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidCommissionPercentage);
    }

    [Fact]
    public async Task UpdateBrokerAsync_CommissionPercentageWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validUpdateBrokerDto with { CommissionPercentage = 5.123m };

        await AssertInvalidUpdateAsync(dto, BrokerErrors.InvalidCommissionPercentageScale);
    }

    [Fact]
    public async Task UpdateBrokerAsync_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var brokerId = TestConstants.NonExistingId;

        _repositoryMock.Setup(x => x.GetBrokerForUpdateAsync(brokerId, It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        // Act
        var result = await _service.UpdateBrokerAsync(brokerId, _validUpdateBrokerDto, CancellationToken.None);

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
        var brokerDto = _validUpdateBrokerDto with
        {
            BrokerCode = "BR002"
        };

        SetupExistingBrokerForUpdate(_existingBroker1);

        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(brokerDto.BrokerCode, _existingBroker1.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateBrokerAsync(_existingBroker1.BrokerId, brokerDto, CancellationToken.None);

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
        SetupExistingBrokerForUpdate(_existingBroker1);
        SetupBrokerCodeDoesNotExistForUpdate();

        // Act
        var result = await _service.UpdateBrokerAsync(_existingBroker1.BrokerId, _validUpdateBrokerDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.BrokerCodeExistsAsync(_validUpdateBrokerDto.BrokerCode,_existingBroker1.BrokerId,It.IsAny<CancellationToken>()),Times.Once);
        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()),Times.Once);
    }

    [Fact]
    public async Task UpdateBrokerAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        SetupExistingBrokerForUpdate(_existingBroker1);
        SetupBrokerCodeDoesNotExistForUpdate();

        _repositoryMock.Setup(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Broker)));

        // Act
        var result = await _service.UpdateBrokerAsync(_existingBroker1.BrokerId, _validUpdateBrokerDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(_validUpdateBrokerDto.BrokerCode).Code,result.Error.Code);
    }

    #endregion

    #region Activate Broker Tests

    [Fact]
    public async Task ActivateBrokerAsync_ExistingBroker_ReturnsActivatedBroker()
    {
        // Arrange
        _existingBroker1.IsActive = false;

        SetupExistingBrokerForUpdate(_existingBroker1);

        // Act
        var result = await _service.ActivateBrokerAsync(_existingBroker1.BrokerId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.IsActive);
        Assert.NotNull(_existingBroker1.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Act
        var result = await _service.ActivateBrokerAsync(Guid.Empty, CancellationToken.None);

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
        var brokerId = TestConstants.NonExistingId;

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
        _existingBroker1.IsActive = true;

        SetupExistingBrokerForUpdate(_existingBroker1);

        // Act
        var result = await _service.DeactivateBrokerAsync(
            _existingBroker1.BrokerId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value.IsActive);
        Assert.NotNull(_existingBroker1.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateBrokerAsync_EmptyBrokerId_ReturnsValidationError()
    {
        // Act
        var result = await _service.DeactivateBrokerAsync(Guid.Empty, CancellationToken.None);

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
        var brokerId = TestConstants.NonExistingId;

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

    private void SetupBrokerCodeDoesNotExistForCreate()
    {
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(_validCreateBrokerDto.BrokerCode, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupBrokerCodeDoesNotExistForUpdate()
    {
        _repositoryMock.Setup(x => x.BrokerCodeExistsAsync(_validUpdateBrokerDto.BrokerCode, _existingBroker1.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private async Task AssertInvalidCreateAsync(CreateBrokerDto dto, Error expectedError)
    {
        var result = await _service.CreateBrokerAsync(dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddBrokerAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidUpdateAsync(UpdateBrokerDto dto, Error expectedError)
    {
        var result = await _service.UpdateBrokerAsync(_existingBroker1.BrokerId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetBrokerForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveBrokerChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
