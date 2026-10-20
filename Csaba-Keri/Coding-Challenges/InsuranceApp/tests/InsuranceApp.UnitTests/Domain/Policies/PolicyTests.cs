using InsuranceApp.Domain.Policies;
using InsuranceApp.Domain.Policies.Exceptions;
using InsuranceApp.UnitTests.TestData.Policies;

namespace InsuranceApp.UnitTests.Domain.Policies;

public sealed class PolicyTests
{
    [Fact]
    public void CreateDraftPolicy_ValidData_SetsDraftIdentityAndTimestamps()
    {
        // Arrange
        var draftPolicyData = PolicyTestData.DefaultDraftPolicyData;
        var now = PolicyTestData.DefaultNow;

        var expectedPolicyNumber = PolicyNumberGenerator.Generate(draftPolicyData.PolicyId);
        var expectedStatus = PolicyStatus.Draft;

        // Act
        var policy = Policy.CreateDraftPolicy(
            data: draftPolicyData,
            now: now
        );

        // Assert
        Assert.Equal(draftPolicyData.PolicyId, policy.Id);
        Assert.Equal(expectedPolicyNumber, policy.PolicyNumber);
        Assert.Equal(expectedStatus, policy.Status);
        Assert.Equal(draftPolicyData.ClientId, policy.ClientId);
        Assert.Equal(draftPolicyData.BuildingId, policy.BuildingId);
        Assert.Equal(draftPolicyData.BrokerId, policy.BrokerId);
        Assert.Equal(draftPolicyData.CurrencyId, policy.CurrencyId);
        Assert.Equal(draftPolicyData.CurrencyExchangeRateToBase, policy.CurrencyExchangeRateToBase);
        Assert.Equal(draftPolicyData.StartDate, policy.StartDate);
        Assert.Equal(draftPolicyData.EndDate, policy.EndDate);
        Assert.Equal(draftPolicyData.BasePremium, policy.BasePremium);
        Assert.Equal(draftPolicyData.FinalPremium, policy.FinalPremium);
        Assert.Equal(draftPolicyData.AppliedAdjustments, policy.AppliedAdjustments);
        Assert.Equal(now, policy.CreatedAt);
        Assert.Equal(now, policy.UpdatedAt);
    }

    [Fact]
    public void CreateDraftPolicy_SourceListChanges_PreservesReadOnlyAdjustments()
    {
        // Arrange
        var adjustment = AppliedAdjustmentTestData.Create();
        var adjustments = new List<AppliedAdjustment> { adjustment };

        var draftPolicyData = PolicyTestData.DefaultDraftPolicyData with
        {
            AppliedAdjustments = adjustments
        };

        // Act
        var policy = Policy.CreateDraftPolicy(
            data: draftPolicyData,
            now: PolicyTestData.DefaultNow
        );

        adjustments.Clear();

        // Assert
        Assert.Equal(adjustment, Assert.Single(policy.AppliedAdjustments));
        Assert.True(Assert.IsType<ICollection<AppliedAdjustment>>(policy.AppliedAdjustments, exactMatch: false).IsReadOnly);
    }

    [Theory]
    [InlineData("PolicyId")]
    [InlineData("ClientId")]
    [InlineData("BuildingId")]
    [InlineData("BrokerId")]
    [InlineData("CurrencyId")]
    [InlineData("ExchangeRate")]
    [InlineData("Period")]
    [InlineData("Premium")]
    public void CreateDraftPolicy_InvalidData_Throws(string invalidField)
    {
        // Arrange
        var draftPolicyData = PolicyTestData.DefaultDraftPolicyData;

        draftPolicyData = invalidField switch
        {
            "PolicyId" => draftPolicyData with { PolicyId = Guid.Empty },
            "ClientId" => draftPolicyData with { ClientId = Guid.Empty },
            "BuildingId" => draftPolicyData with { BuildingId = Guid.Empty },
            "BrokerId" => draftPolicyData with { BrokerId = Guid.Empty },
            "CurrencyId" => draftPolicyData with { CurrencyId = Guid.Empty },
            "ExchangeRate" => draftPolicyData with { CurrencyExchangeRateToBase = 0m },
            "Period" => draftPolicyData with { EndDate = draftPolicyData.StartDate.AddDays(-1) },
            "Premium" => draftPolicyData with { FinalPremium = PolicyRules.MinFinalPremium - 0.01m },

            _ => draftPolicyData
        };

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(
            () => Policy.CreateDraftPolicy(
                data: draftPolicyData,
                now: PolicyTestData.DefaultNow
            )
        );
    }

    [Fact]
    public void CreateDraftPolicy_DuplicateAdjustment_Throws()
    {
        // Arrange
        var adjustment = AppliedAdjustmentTestData.Create();

        var draftPolicyData = PolicyTestData.DefaultDraftPolicyData with
        {
            AppliedAdjustments = [adjustment, adjustment]
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => Policy.CreateDraftPolicy(
                data: draftPolicyData,
                now: PolicyTestData.DefaultNow
            )
        );
    }

    [Fact]
    public void Activate_Draft_PreservesPricingAndSetsAudit()
    {
        // Arrange
        var data = PolicyTestData.DefaultPolicyData;
        var policy = PolicyTestData.Create(data);
        var adjustments = policy.AppliedAdjustments;
        var now = PolicyTestData.DefaultNow;

        // Act
        policy.Activate(now);

        // Assert
        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.Equal(now, policy.ActivatedAt);
        Assert.Equal(now, policy.UpdatedAt);
        Assert.Equal(data.CreatedAt, policy.CreatedAt);
        Assert.Equal(data.BasePremium, policy.BasePremium);
        Assert.Equal(data.FinalPremium, policy.FinalPremium);
        Assert.Equal(data.CurrencyExchangeRateToBase, policy.CurrencyExchangeRateToBase);
        Assert.Same(adjustments, policy.AppliedAdjustments);
    }

    [Theory]
    [InlineData(PolicyStatus.Active)]
    [InlineData(PolicyStatus.Cancelled)]
    [InlineData(PolicyStatus.Expired)]
    public void Activate_NonDraft_Throws(PolicyStatus status)
    {
        // Arrange
        var data = PolicyTestData.DefaultPolicyData with { Status = status };
        var policy = PolicyTestData.Create(data);
        var now = PolicyTestData.DefaultNow;

        // Act & Assert
        Assert.Throws<PolicyStateConflictException>(
            () => policy.Activate(now)
        );
        
        Assert.Equal(status, policy.Status);
    }

    [Fact]
    public void Activate_PastStartDate_ThrowsWithoutMutation()
    {
        // Arrange
        var policy = PolicyTestData.Create();
        var now = PolicyTestData.DefaultNow.AddDays(1);

        // Act & Assert
        Assert.Throws<PolicyStateConflictException>(
            () => policy.Activate(now)
        );
        
        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Null(policy.ActivatedAt);
    }

    [Fact]
    public void Cancel_Active_TrimsReasonAndPreservesActivationAndPricing()
    {
        // Arrange
        var policy = PolicyTestData.CreateAndActive();
        var now = PolicyTestData.DefaultNow.AddMinutes(1);
        var reason = PolicyTestData.DefaultCancellationReason;
        var effectiveDate = PolicyTestData.DefaultToday;

        // Act
        policy.Cancel(
            reason: $"  {reason}  ",
            effectiveDate: effectiveDate,
            now: now
        );

        // Assert
        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
        Assert.Equal(reason, policy.CancellationReason);
        Assert.Equal(effectiveDate, policy.CancellationEffectiveDate);
        Assert.Equal(now, policy.CancelledAt);
        Assert.Equal(now, policy.UpdatedAt);
        Assert.Equal(PolicyTestData.DefaultNow, policy.ActivatedAt);
        Assert.Equal(PolicyTestData.DefaultFinalPremium, policy.FinalPremium);
    }

    [Fact]
    public void Cancel_FutureStartDate_IsAllowed()
    {
        // Arrange
        var data = PolicyTestData.DefaultPolicyData with
        {
            StartDate = PolicyTestData.DefaultToday.AddDays(1)
        };

        var policy = PolicyTestData.CreateAndActive(data);

        // Act
        policy.Cancel(
            reason: PolicyTestData.DefaultCancellationReason,
            effectiveDate: PolicyTestData.DefaultToday,
            now: PolicyTestData.DefaultNow
        );

        // Assert
        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
    }

    [Theory]
    [InlineData(PolicyStatus.Draft)]
    [InlineData(PolicyStatus.Cancelled)]
    [InlineData(PolicyStatus.Expired)]
    public void Cancel_NonActive_Throws(PolicyStatus status)
    {
        // Arrange
        var data = PolicyTestData.DefaultPolicyData with { Status = status };
        var policy = PolicyTestData.Create(data);

        // Act & Assert
        Assert.Throws<PolicyStateConflictException>(
            () => policy.Cancel(
                reason: PolicyTestData.DefaultCancellationReason,
                effectiveDate: PolicyTestData.DefaultToday,
                now: PolicyTestData.DefaultNow
            )
        );
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void Cancel_EffectiveDateNotToday_ThrowsWithoutMutation(int dayOffset)
    {
        // Arrange
        var policy = PolicyTestData.CreateAndActive();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => policy.Cancel(
                reason: PolicyTestData.DefaultCancellationReason,
                effectiveDate: PolicyTestData.DefaultToday.AddDays(dayOffset),
                now: PolicyTestData.DefaultNow
            )
        );
        
        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.Null(policy.CancelledAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_InvalidReason_ThrowsWithoutMutation(string reason)
    {
        // Arrange
        var policy = PolicyTestData.CreateAndActive();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => policy.Cancel(
                reason: reason,
                effectiveDate: PolicyTestData.DefaultToday,
                now: PolicyTestData.DefaultNow
            )
        );

        Assert.Equal(PolicyStatus.Active, policy.Status);
    }

    [Fact]
    public void Cancel_AfterEndDate_Throws()
    {
        // Arrange
        var policy = PolicyTestData.CreateAndActive();
        var now = PolicyTestData.DefaultNow.AddYears(1);

        // Act & Assert
        Assert.Throws<PolicyStateConflictException>(
            () => policy.Cancel(
                reason: PolicyTestData.DefaultCancellationReason,
                effectiveDate: DateOnly.FromDateTime(now.UtcDateTime),
                now: now
            )
        );
    }

    [Fact]
    public void Cancel_OnEndDate_IsAllowed()
    {
        // Arrange
        var policy = PolicyTestData.CreateAndActive();
        var now = new DateTimeOffset(policy.EndDate, new TimeOnly(23, 59), TimeSpan.Zero);

        // Act
        policy.Cancel(
            reason: PolicyTestData.DefaultCancellationReason,
            effectiveDate: policy.EndDate,
            now: now
        );

        // Assert
        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
    }
}
