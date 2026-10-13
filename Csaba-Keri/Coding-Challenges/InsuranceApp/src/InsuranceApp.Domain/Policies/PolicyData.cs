namespace InsuranceApp.Domain.Policies;

public record PolicyData : DraftPolicyData
{
    public required string PolicyNumber { get; init; }
    public required PolicyStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
