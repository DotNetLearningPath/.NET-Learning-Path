using InsuranceApp.Domain.Policies;
using InsuranceApp.UnitTests.TestData.Policies;

namespace InsuranceApp.UnitTests.Domain.Policies;

public sealed class PolicyTests
{
    [Fact]
    public void CreateDraftPolicy_ValidData_SetsDraftIdentityAndTimestamps()
    {
        // Arrange
        var draftPolicyData = PolicyTestData.DefaultDraftPolicyData;
        var now = PolicyTestData.DefaultCreatedAt;
        
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
            now: PolicyTestData.DefaultCreatedAt
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
            "Premium" => draftPolicyData with { FinalPremium = 0m },

            _ => draftPolicyData
        };

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(
            () => Policy.CreateDraftPolicy(
                data: draftPolicyData,
                now: PolicyTestData.DefaultCreatedAt
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
                now: PolicyTestData.DefaultCreatedAt
            )
        );
    }
}
