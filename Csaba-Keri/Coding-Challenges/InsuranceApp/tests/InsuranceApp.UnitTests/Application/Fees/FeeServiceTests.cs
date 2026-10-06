using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Fees;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Domain.Fees;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InsuranceApp.UnitTests.Application.Fees;

public sealed class FeeServiceTests
{
    private readonly IFeeRepository _feeRepository;
    private readonly IValidator<CreateFeeCommand> _createValidator;
    private readonly IValidator<UpdateFeeCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly ILogger<FeeService> _logger;

    private readonly FeeService _feeService;

    private static readonly DateOnly Start = new(2030, 1, 10);

    public FeeServiceTests()
    {
        _feeRepository = Substitute.For<IFeeRepository>();

        _createValidator = ValidatorMocks.CreatePassing<CreateFeeCommand>();
        _updateValidator = ValidatorMocks.CreatePassing<UpdateFeeCommand>();
        _pageValidator = ValidatorMocks.CreatePassing<PageQuery>();

        _logger = Substitute.For<ILogger<FeeService>>();

        _feeService = new(_feeRepository, _createValidator, _updateValidator, _pageValidator, _logger);
    }

    [Fact]
    public async Task CreateFeeAsync_ValidCommand_SavesFeeAndReturnsResult()
    {
        // Arrange
        var name = "Fee";

        var command = CreateCommand(
            name: $" {name} ",
            type: FeeType.BrokerCommission,
            percentage: 3.1234m,
            effectiveFrom: Start,
            effectiveTo: Start.AddDays(30),
            isActive: true
        );

        // Act
        var result = await _feeService.CreateFeeAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal(command.Percentage, result.Percentage);
        Assert.Equal(command.EffectiveFrom, result.EffectiveFrom);
        Assert.Equal(command.EffectiveTo, result.EffectiveTo);
        Assert.True(result.IsActive);

        await _createValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _feeRepository.Received(1).AddFeeAsync(
            Arg.Is<FeeConfiguration>(received =>
                received.Id == result.Id
                && received.Name == name
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task CreateFeeAsync_ValidationFails_DoesNotAccessRepository()
    {
        // Arrange
        var invalidCommand = CreateCommand(percentage: -1m);
        var exception = new ValidationException("Invalid percentage.");

        _createValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _feeService.CreateFeeAsync(invalidCommand, CancellationToken.None)
        );
        
        Assert.Empty(_feeRepository.ReceivedCalls());
    }

    [Fact]
    public async Task UpdateFeeAsync_ValidCommand_UpdatesFieldsAndPreservesId()
    {
        // Arrange
        var fee = CreateDomainFee();
        
        _feeRepository.GetFeeByIdAsync(fee.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<FeeConfiguration?>(fee));

        var command = UpdateCommand(
            feeId: fee.Id,
            name: "Updated",
            type: FeeType.RiskAdjustment,
            percentage: 10m,
            effectiveFrom: Start.AddDays(1),
            effectiveTo: Start.AddDays(10),
            isActive: false
        );

        // Act
        var result = await _feeService.UpdateFeeAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(fee.Id, result.Id);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal(command.Percentage, result.Percentage);
        Assert.Equal(command.EffectiveFrom, result.EffectiveFrom);
        Assert.Equal(command.EffectiveTo, result.EffectiveTo);
        Assert.False(result.IsActive);

        await _updateValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _feeRepository.Received(1).UpdateFeeAsync(fee, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateFeeAsync_ValidationFails_DoesNotAccessRepository()
    {
        // Arrange
        var invalidCommand = UpdateCommand(
            feeId: Guid.NewGuid(),
            percentage: -1m
        );

        var exception = new ValidationException("Invalid percentage.");

        _updateValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _feeService.UpdateFeeAsync(invalidCommand, CancellationToken.None)
        );
        
        Assert.Empty(_feeRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetFeeByIdAsync_ExistingId_ReturnsDetails()
    {
        // Arrange
        var fee = CreateDomainFee();

        _feeRepository.GetFeeByIdAsync(fee.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<FeeConfiguration?>(fee));

        // Act
        var result = await _feeService.GetFeeByIdAsync(fee.Id, CancellationToken.None);

        // Assert
        Assert.Equal(fee.Id, result.Id);
        Assert.Equal(fee.Name, result.Name);
        Assert.Equal(fee.Type, result.Type);
        Assert.Equal(fee.Percentage, result.Percentage);
        Assert.Equal(fee.EffectiveFrom, result.EffectiveFrom);
        Assert.Null(result.EffectiveTo);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetFeeByIdAsync_UnknownId_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _feeService.GetFeeByIdAsync(id, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetFeeByIdAsync_EmptyId_DoesNotAccessRepository()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _feeService.GetFeeByIdAsync(id, CancellationToken.None)
        );
        
        Assert.Empty(_feeRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetFeesAsync_ValidQuery_PreservesPagination()
    {
        // Arrange
        var fee = CreateDomainFee();

        var pageNumber = 2;
        var pageSize = 1;
        var totalCount = 3L;

        var query = new PageQuery(pageNumber, pageSize);
        var page = new PagedResult<FeeConfiguration>([fee], pageNumber, pageSize, totalCount);

        _feeRepository.GetFeesAsync(query, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(page));

        // Act
        var result = await _feeService.GetFeesAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(fee.Id, Assert.Single(result.Items).Id);

        await _pageValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, query) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
    }

    private static CreateFeeCommand CreateCommand(
        string name = "Fee",
        FeeType type = FeeType.AdminFee,
        decimal percentage = 1m,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        bool isActive = true
    )
    {
        return new(
            Name: name,
            Type: type,
            Percentage: percentage,
            EffectiveFrom: effectiveFrom ?? Start,
            EffectiveTo: effectiveTo,
            IsActive: isActive
        );
    }

    private static UpdateFeeCommand UpdateCommand(
        Guid feeId,
        string name = "Updated",
        FeeType type = FeeType.RiskAdjustment,
        decimal percentage = 10m,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        bool isActive = true
    )
    {
        return new(
            FeeId: feeId,
            Name: name,
            Type: type,
            Percentage: percentage,
            EffectiveFrom: effectiveFrom ?? Start.AddDays(1),
            EffectiveTo: effectiveTo,
            IsActive: isActive
        );
    }

    private static FeeConfiguration CreateDomainFee(
        Guid? feeId = null,
        string name = "Fee",
        FeeType type = FeeType.AdminFee,
        decimal percentage = 1m,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        bool isActive = true
    )
    {
        return new(
            id: feeId ?? Guid.NewGuid(),
            name: name,
            type: type,
            percentage: percentage,
            effectiveFrom: effectiveFrom ?? Start,
            effectiveTo: effectiveTo,
            isActive: isActive
        );
    }
}
