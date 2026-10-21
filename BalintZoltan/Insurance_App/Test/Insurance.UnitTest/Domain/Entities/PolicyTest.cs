using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Domain.Entities;

public class PolicyTest
{
    private static Policy CreatePolicy(
        PolicyStatus status = PolicyStatus.Active,
        DateTime? startDate = null)
    {
        var effectiveStartDate = startDate ??
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return new Policy(
            "POL-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            effectiveStartDate,
            effectiveStartDate.AddYears(1),
            100m,
            120m,
            status);
    }

    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        // Arrange & Act
        var policy = CreatePolicy(PolicyStatus.Draft);

        // Assert
        Assert.NotEqual(Guid.Empty, policy.Id);
        Assert.Equal("POL-001", policy.PolicyNumber);
        Assert.NotEqual(Guid.Empty, policy.ClientId);
        Assert.NotEqual(Guid.Empty, policy.BuildingId);
        Assert.NotEqual(Guid.Empty, policy.BrokerId);
        Assert.NotEqual(Guid.Empty, policy.CurrencyId);
        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Equal(100m, policy.BasePremium);
        Assert.Equal(120m, policy.FinalPremium);
        Assert.Equal(policy.CreatedAt, policy.UpdatedAt);
        Assert.Null(policy.CancellationDate);
        Assert.Null(policy.CancellationReason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_Policy_Number_Is_Invalid(string? policyNumber)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var buildingId = Guid.NewGuid();
        var brokerId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddYears(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Policy(
            policyNumber!,
            clientId,
            buildingId,
            brokerId,
            currencyId,
            startDate,
            endDate,
            100m,
            120m,
            PolicyStatus.Draft));
    }

    [Theory]
    [InlineData("ClientId")]
    [InlineData("BuildingId")]
    [InlineData("BrokerId")]
    [InlineData("CurrencyId")]
    public void Constructor_Should_Throw_When_Identifier_Is_Empty(string invalidIdentifier)
    {
        // Arrange
        var clientId = invalidIdentifier == "ClientId" ? Guid.Empty : Guid.NewGuid();
        var buildingId = invalidIdentifier == "BuildingId" ? Guid.Empty : Guid.NewGuid();
        var brokerId = invalidIdentifier == "BrokerId" ? Guid.Empty : Guid.NewGuid();
        var currencyId = invalidIdentifier == "CurrencyId" ? Guid.Empty : Guid.NewGuid();
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddYears(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Policy(
            "POL-001",
            clientId,
            buildingId,
            brokerId,
            currencyId,
            startDate,
            endDate,
            100m,
            120m,
            PolicyStatus.Draft));
    }
    [Fact]
    public void Activate_Should_Set_Active_Status_When_Start_Date_Is_Valid()
    {
        // Arrange
        var nowUtc = DateTime.UtcNow;
        var policy = CreatePolicy(
            PolicyStatus.Draft,
            nowUtc.Date.AddDays(1));

        // Act
        policy.Activate(nowUtc);

        // Assert
        Assert.Equal(PolicyStatus.Active, policy.Status);
    }

    [Theory]
    [InlineData(PolicyStatus.Active)]
    [InlineData(PolicyStatus.Expired)]
    [InlineData(PolicyStatus.Cancelled)]
    public void Activate_Should_Throw_When_Policy_Is_Not_Draft(PolicyStatus status)
    {
        // Arrange
        var policy = CreatePolicy(status, DateTime.UtcNow.Date.AddDays(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            policy.Activate(DateTime.UtcNow));
    }

    [Fact]
    public void Activate_Should_Throw_When_Start_Date_Is_In_The_Past()
    {
        // Arrange
        var policy = CreatePolicy(
            PolicyStatus.Draft,
            DateTime.UtcNow.Date.AddDays(-1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            policy.Activate(DateTime.UtcNow));
    }

    [Fact]
    public void Cancel_Should_Set_Cancelled_Status_And_Store_Details()
    {
        // Arrange
        var policy = CreatePolicy();
        var cancellationDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        policy.Cancel("Customer request", cancellationDate);

        // Assert
        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
        Assert.Equal(cancellationDate, policy.CancellationDate);
        Assert.Equal("Customer request", policy.CancellationReason);
    }

    [Fact]
    public void Cancel_Should_Throw_When_Policy_Is_Not_Active()
    {
        // Arrange
        var policy = CreatePolicy(PolicyStatus.Draft);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            policy.Cancel("Customer request", new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Cancel_Should_Throw_When_Reason_Is_Invalid(string? reason)
    {
        // Arrange
        var policy = CreatePolicy();
        var cancellationDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            policy.Cancel(reason!, cancellationDate));
    }
}
