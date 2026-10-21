namespace Insurance.Application.DTO.Policies;

public sealed class CancelPolicyRequest
{
    public string Reason { get; set; } = string.Empty;
    public DateTime CancellationDate { get; set; }
}
