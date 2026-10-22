namespace InsuranceApp.Domain.Policies;

public record PolicyData : DraftPolicyData
{
    public required string PolicyNumber { get; init; }
    public required PolicyStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public DateOnly? CancellationEffectiveDate { get; init; }
    public string? CancellationReason { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }
}
