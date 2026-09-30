using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Brokers;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Exceptions;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InsuranceApp.UnitTests.Application.Brokers;

public sealed class BrokerServiceTests
{
    private readonly IBrokerRepository _brokerRepository;
    private readonly IValidator<CreateBrokerCommand> _createValidator;
    private readonly IValidator<UpdateBrokerCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly ILogger<BrokerService> _logger;

    private readonly BrokerService _brokerService;

    public BrokerServiceTests()
    {
        _brokerRepository = Substitute.For<IBrokerRepository>();

        _createValidator = ValidatorMocks.CreatePassing<CreateBrokerCommand>();
        _updateValidator = ValidatorMocks.CreatePassing<UpdateBrokerCommand>();
        _pageValidator = ValidatorMocks.CreatePassing<PageQuery>();

        _logger = Substitute.For<ILogger<BrokerService>>();

        _brokerService = new(_brokerRepository, _createValidator, _updateValidator, _pageValidator, _logger);
    }

    [Fact]
    public async Task CreateBrokerAsync_WhenValid_SavesBrokerAndReturnsResult()
    {
        // Arrange
        var command = CreateCommand(code: " br-001 ");

        // Act
        var result = await _brokerService.CreateBrokerAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("BR-001", result.Code);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Phone, result.Phone);
        Assert.Equal(command.Status, result.Status);

        await _createValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _brokerRepository.Received(1).AddBrokerAsync(
            Arg.Is<Broker>(received =>
                received.Id == result.Id
                && received.Code == "BR-001"
                && received.Name == command.Name
                && received.Email == command.Email
                && received.Phone == command.Phone
                && received.Status == command.Status
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task CreateBrokerAsync_WhenRepositoryReportsDuplicate_ThrowsWithoutSaving()
    {
        // Arrange
        var command = CreateCommand();

        _brokerRepository.AddBrokerAsync(Arg.Any<Broker>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateBrokerCodeException());

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateBrokerCodeException>(
            () => _brokerService.CreateBrokerAsync(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task CreateBrokerAsync_WhenValidationFails_ThrowsWithoutRepositoryCalls()
    {
        // Arrange
        var command = CreateCommand();
        var failure = new ValidationException("Rejected by the validator mock.");

        _createValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(failure));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _brokerService.CreateBrokerAsync(command, CancellationToken.None)
        );

        Assert.Same(failure, exception);
        Assert.Empty(_brokerRepository.ReceivedCalls());
    }

    [Fact]
    public async Task UpdateBrokerAsync_WhenBrokerExists_SavesOnlyDetails()
    {
        // Arrange
        var broker = CreateDomainBroker();
        var command = UpdateCommand(broker.Id);

        _brokerRepository.GetBrokerByIdAsync(broker.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Broker?>(broker));

        // Act
        var result = await _brokerService.UpdateBrokerAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(broker.Id, result.Id);
        Assert.Equal(broker.Code, result.Code);
        Assert.Equal(broker.Status, result.Status);

        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Phone, result.Phone);

        await _updateValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _brokerRepository.Received(1).UpdateBrokerAsync(
            Arg.Is<Broker>(received =>
                received.Id == command.BrokerId
                && received.Code == broker.Code
                && received.Name == command.Name
                && received.Email == command.Email
                && received.Phone == command.Phone
                && received.Status == broker.Status
            ),
            CancellationToken.None
        );

        await _brokerRepository.DidNotReceive().AddBrokerAsync(Arg.Any<Broker>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBrokerByIdAsync_WhenBrokerExists_ReturnsBrokerResult()
    {
        // Arrange
        var broker = CreateDomainBroker();

        _brokerRepository.GetBrokerByIdAsync(broker.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Broker?>(broker)
        );

        // Act
        var result = await _brokerService.GetBrokerByIdAsync(broker.Id, CancellationToken.None);

        // Assert
        Assert.Equal(broker.Id, result.Id);
        Assert.Equal(broker.Code, result.Code);
        Assert.Equal(broker.Name, result.Name);
        Assert.Equal(broker.Email, result.Email);
        Assert.Equal(broker.Phone, result.Phone);
        Assert.Equal(broker.Status, result.Status);

        await _brokerRepository.Received(1).GetBrokerByIdAsync(broker.Id, CancellationToken.None);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_WhenIdIsUnknown_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _brokerService.GetBrokerByIdAsync(id, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetBrokersAsync_WhenValid_ReturnsMappedPage()
    {
        // Arrange
        var broker = CreateDomainBroker();

        var pageNumber = 2;
        var pageSize = 1;
        var totalCount = 3L;

        var query = new PageQuery(pageNumber, pageSize);
        var page = new PagedResult<Broker>([broker], pageNumber, pageSize, totalCount);

        _brokerRepository.GetBrokersAsync(Arg.Any<PageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(page));

        // Act
        var result = await _brokerService.GetBrokersAsync(query, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Items);
        Assert.Equal(broker.Id, item.Id);
        Assert.Equal(broker.Code, item.Code);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);

        await _pageValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, query) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _brokerRepository.Received(1).GetBrokersAsync(
            Arg.Is<PageQuery>(received =>
                received.PageNumber == pageNumber
                && received.PageSize == pageSize
            ),
            CancellationToken.None
        );
    }

    [Theory]
    [InlineData(BrokerStatus.Inactive, BrokerStatus.Active)]
    [InlineData(BrokerStatus.Active, BrokerStatus.Inactive)]
    public async Task ChangeBrokerStatusAsync_WhenBrokerExists_SavesOnlyStatus(BrokerStatus originalStatus, BrokerStatus expectedStatus)
    {
        // Arrange
        var broker = CreateDomainBroker(status: originalStatus);
        
        _brokerRepository.GetBrokerByIdAsync(broker.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Broker?>(broker));

        // Act
        var result = expectedStatus switch
        {
            BrokerStatus.Active => await _brokerService.ActivateBrokerAsync(broker.Id, CancellationToken.None),
            BrokerStatus.Inactive => await _brokerService.DeactivateBrokerAsync(broker.Id, CancellationToken.None),

            _ => throw new ArgumentOutOfRangeException(
                nameof(expectedStatus),
                expectedStatus,
                "Unsupported broker status."
            )
        };

        // Assert
        Assert.Equal(expectedStatus, result.Status);

        await _brokerRepository.Received(1).UpdateBrokerAsync(
            Arg.Is<Broker>(received =>
                received.Id == broker.Id
                && received.Code == broker.Code
                && received.Name == broker.Name
                && received.Email == broker.Email
                && received.Phone == broker.Phone
                && received.Status == expectedStatus
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task ActivateBrokerAsync_WhenIdIsEmpty_ThrowsWithoutRepositoryCalls()
    {
        // Arrange
        var brokerId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _brokerService.ActivateBrokerAsync(brokerId, CancellationToken.None)
        );

        Assert.Equal("BrokerId", Assert.Single(exception.Errors).PropertyName);
        Assert.Empty(_brokerRepository.ReceivedCalls());
    }

    [Fact]
    public async Task DeactivateBrokerAsync_WhenIdIsEmpty_ThrowsWithoutRepositoryCalls()
    {
        // Arrange
        var brokerId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _brokerService.DeactivateBrokerAsync(brokerId, CancellationToken.None)
        );

        Assert.Equal("BrokerId", Assert.Single(exception.Errors).PropertyName);
        Assert.Empty(_brokerRepository.ReceivedCalls());
    }

    private static CreateBrokerCommand CreateCommand(
        string code = "BR-001",
        string name = "Broker",
        string email = "broker@example.com",
        string phone = "123",
        BrokerStatus status = BrokerStatus.Active
    )
    {
        return new(
            Code: code,
            Name: name,
            Email: email,
            Phone: phone,
            Status: status
        );
    }

    private static UpdateBrokerCommand UpdateCommand(
        Guid brokerId,
        string name = "Updated",
        string email = "updated@example.com",
        string phone = "456"
    )
    {
        return new(
            BrokerId: brokerId,
            Name: name,
            Email: email,
            Phone: phone
        );
    }

    private static Broker CreateDomainBroker(
        Guid? id = null,
        string code = "BR-001",
        string name = "Broker",
        string email = "broker@example.com",
        string phone = "123",
        BrokerStatus status = BrokerStatus.Active
    )
    {
        return new(
            id: id ?? Guid.NewGuid(),
            code: code,
            name: name,
            email: email,
            phone: phone,
            status: status
        );
    }
}
