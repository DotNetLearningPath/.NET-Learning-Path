using InsuranceApp.Domain.Currencies;
using InsuranceApp.Domain.Policies.Exceptions;

namespace InsuranceApp.Domain.Policies;

public class Policy
{
    public Guid Id { get; }
    public string PolicyNumber { get; }
    public PolicyStatus Status { get; private set; }
    public Guid ClientId { get; }
    public Guid BuildingId { get; }
    public Guid BrokerId { get; }
    public Guid CurrencyId { get; }
    public decimal CurrencyExchangeRateToBase { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public decimal BasePremium { get; }
    public decimal FinalPremium { get; }
    public IReadOnlyList<AppliedAdjustment> AppliedAdjustments { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateOnly? CancellationEffectiveDate { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }

    public Policy(PolicyData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        ValidateIdentifier(data.PolicyId, nameof(data.PolicyId));
        ValidateIdentifier(data.ClientId, nameof(data.ClientId));
        ValidateIdentifier(data.BuildingId, nameof(data.BuildingId));
        ValidateIdentifier(data.BrokerId, nameof(data.BrokerId));
        ValidateIdentifier(data.CurrencyId, nameof(data.CurrencyId));

        ValidatePolicyNumber(data.PolicyNumber);
        ValidateStatus(data.Status);
        ValidateCurrencyExchangeRate(data.CurrencyExchangeRateToBase);
        ValidatePolicyPeriod(data.StartDate, data.EndDate);
        ValidateTimestamps(data.CreatedAt, data.UpdatedAt);
        ValidateLifecycleData(data);

        ValidateBasePremium(data.BasePremium);
        ValidateFinalPremium(data.FinalPremium);

        var adjustments = CopyAndValidateAdjustments(
            data.AppliedAdjustments
        );

        Id = data.PolicyId;
        ClientId = data.ClientId;
        BuildingId = data.BuildingId;
        BrokerId = data.BrokerId;
        CurrencyId = data.CurrencyId;

        PolicyNumber = data.PolicyNumber.Trim();
        Status = data.Status;
        CurrencyExchangeRateToBase = data.CurrencyExchangeRateToBase;

        StartDate = data.StartDate;
        EndDate = data.EndDate;

        CreatedAt = data.CreatedAt;
        UpdatedAt = data.UpdatedAt;
        ActivatedAt = data.ActivatedAt;
        CancelledAt = data.CancelledAt;
        CancellationEffectiveDate = data.CancellationEffectiveDate;
        CancellationReason = data.CancellationReason;
        ExpiredAt = data.ExpiredAt;

        BasePremium = data.BasePremium;
        FinalPremium = data.FinalPremium;

        AppliedAdjustments = adjustments;
    }

    public static Policy CreateDraftPolicy(DraftPolicyData data, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new Policy(new PolicyData
        {
            PolicyId = data.PolicyId,
            ClientId = data.ClientId,
            BuildingId = data.BuildingId,
            BrokerId = data.BrokerId,
            CurrencyId = data.CurrencyId,

            CurrencyExchangeRateToBase = data.CurrencyExchangeRateToBase,

            StartDate = data.StartDate,
            EndDate = data.EndDate,

            BasePremium = data.BasePremium,
            FinalPremium = data.FinalPremium,
            AppliedAdjustments = data.AppliedAdjustments,

            PolicyNumber = PolicyNumberGenerator.Generate(data.PolicyId),
            Status = PolicyStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public void Activate(DateTimeOffset now)
    {
        EnsureStatus(PolicyStatus.Draft);
        ValidateTransitionTime(now);

        if (StartDate < DateOnly.FromDateTime(now.UtcDateTime))
        {
            throw new PolicyStateConflictException("A policy with a past start date cannot be activated.");
        }

        Status = PolicyStatus.Active;
        ActivatedAt = now;
        UpdatedAt = now;
    }

    public void Cancel(string reason, DateOnly effectiveDate, DateTimeOffset now)
    {
        EnsureStatus(PolicyStatus.Active);
        ValidateTransitionTime(now);
        
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        if (EndDate < today)
        {
            throw new PolicyStateConflictException("An elapsed policy cannot be cancelled.");
        }

        if (!PolicyRules.IsValidCancellationReason(reason))
        {
            throw new ArgumentException("A valid cancellation reason is required.", nameof(reason));
        }

        if (effectiveDate != today)
        {
            throw new ArgumentException("Cancellation must take effect today (UTC).", nameof(effectiveDate));
        }

        Status = PolicyStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancellationEffectiveDate = effectiveDate;
        CancelledAt = now;
        UpdatedAt = now;
    }

    private static void ValidateIdentifier(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier must not be empty.",
                parameterName
            );
        }
    }

    private static void ValidatePolicyNumber(string policyNumber)
    {
        if (!PolicyRules.IsValidPolicyNumber(policyNumber))
        {
            throw new ArgumentException(
                $"Policy number is required and must not exceed {PolicyRules.MaxPolicyNumberLength} characters.",
                nameof(policyNumber)
            );
        }
    }

    private static void ValidateStatus(PolicyStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Policy status is invalid."
            );
        }
    }

    private static void ValidateCurrencyExchangeRate(decimal rate)
    {
        if (!CurrencyRules.IsValidExchangeRate(rate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rate),
                rate,
                "Currency exchange rate is invalid."
            );
        }
    }

    private static void ValidatePolicyPeriod(DateOnly startDate, DateOnly endDate)
    {
        if (!PolicyRules.IsValidPeriod(startDate, endDate))
        {
            throw new ArgumentException(
                "Start date must be specified and end date must not precede it.",
                nameof(endDate)
            );
        }
    }

    private static void ValidateBasePremium(decimal basePremium)
    {
        if (!PolicyRules.IsValidBasePremium(basePremium))
        {
            throw new ArgumentOutOfRangeException(
                nameof(basePremium),
                basePremium,
                $"Base premium must be greater than 0, at most {PolicyRules.MaxPremium}" +
                $", and have at most {PolicyRules.MoneyScale} decimal places."
            );
        }
    }

    private static void ValidateFinalPremium(decimal finalPremium)
    {
        if (!PolicyRules.IsValidFinalPremium(finalPremium))
        {
            throw new ArgumentOutOfRangeException(
                nameof(finalPremium),
                finalPremium,
                $"Final premium must be between {PolicyRules.MinFinalPremium} and {PolicyRules.MaxPremium}" +
                $", and have at most {PolicyRules.MoneyScale} decimal places."
            );
        }
    }

    private static void ValidateTimestamps(DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        if (createdAt == default || createdAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Creation time must be a valid UTC timestamp.",
                nameof(createdAt)
            );
        }

        if (updatedAt.Offset != TimeSpan.Zero || updatedAt < createdAt)
        {
            throw new ArgumentException(
                "Update time must be a UTC timestamp at or after creation.",
                nameof(updatedAt)
            );
        }
    }

    private static IReadOnlyList<AppliedAdjustment> CopyAndValidateAdjustments(
        IEnumerable<AppliedAdjustment> appliedAdjustments
    )
    {
        ArgumentNullException.ThrowIfNull(appliedAdjustments);

        var adjustments = appliedAdjustments.ToArray();

        if (adjustments.Any(adjustment => adjustment is null))
        {
            throw new ArgumentException(
                "Applied adjustments must not contain null entries.",
                nameof(appliedAdjustments)
            );
        }

        var distinctCount = adjustments
            .Select(adjustment => (
                adjustment.Source,
                adjustment.ConfigurationId
            ))
            .Distinct()
            .Count();

        if (distinctCount != adjustments.Length)
        {
            throw new ArgumentException(
                "A configuration may only be applied once.",
                nameof(appliedAdjustments)
            );
        }

        return Array.AsReadOnly(adjustments);
    }

    private static void ValidateLifecycleData(PolicyData data)
    {
        ValidateAuditTimestamp(data.ActivatedAt, data);
        ValidateAuditTimestamp(data.CancelledAt, data);
        ValidateAuditTimestamp(data.ExpiredAt, data);

        if (data.Status == PolicyStatus.Draft && data.ActivatedAt is not null)
        {
            throw new ArgumentException(
                "A draft policy cannot have an activation timestamp.",
                nameof(data)
            );
        }

        ValidateCancellationData(data);

        if (data.ExpiredAt is { } expiredAt
            && (data.Status != PolicyStatus.Expired
                || data.EndDate >= DateOnly.FromDateTime(expiredAt.UtcDateTime)
                || (data.ActivatedAt is { } activatedAt && expiredAt < activatedAt)))
        {
            throw new ArgumentException(
                "Expiry audit data is inconsistent.",
                nameof(data)
            );
        }
    }

    private static void ValidateAuditTimestamp(DateTimeOffset? timestamp, PolicyData data)
    {
        if (timestamp is { } value
            && (value.Offset != TimeSpan.Zero || value < data.CreatedAt || value > data.UpdatedAt))
        {
            throw new ArgumentException(
                "Lifecycle timestamps must be UTC and between creation and last update.",
                nameof(data)
            );
        }
    }

    private static void ValidateCancellationData(PolicyData data)
    {
        if (data.CancelledAt is null && data.CancellationEffectiveDate is null && data.CancellationReason is null)
        {
            return;
        }

        if (data.Status != PolicyStatus.Cancelled
            || data.CancelledAt is not { } cancelledAt
            || data.CancellationEffectiveDate is not { } effectiveDate
            || data.CancellationReason is not { } reason
            || !PolicyRules.IsValidCancellationReason(reason)
            || effectiveDate != DateOnly.FromDateTime(cancelledAt.UtcDateTime)
            || effectiveDate > data.EndDate
            || (data.ActivatedAt is { } activatedAt && cancelledAt < activatedAt))
        {
            throw new ArgumentException(
                "Cancellation audit data is inconsistent.",
                nameof(data)
            );
        }
    }

    private void EnsureStatus(PolicyStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new PolicyStateConflictException(
                $"This operation requires {expectedStatus} policy; current status is {Status}."
            );
        }
    }

    private void ValidateTransitionTime(DateTimeOffset now)
    {
        if (now == default || now.Offset != TimeSpan.Zero || now < UpdatedAt)
        {
            throw new ArgumentException(
                "Transition time must be UTC and not precede the last update.",
                nameof(now)
            );
        }
    }
}
