using InsuranceApp.Domain.Currencies;

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

        ValidatePremium(data.BasePremium, nameof(data.BasePremium));
        ValidatePremium(data.FinalPremium, nameof(data.FinalPremium));

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

            PolicyNumber = $"POL-{data.PolicyId:N}".ToUpperInvariant(),
            Status = PolicyStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        });
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

    private static void ValidatePremium(decimal premium, string parameterName)
    {
        if (!PolicyRules.IsValidPremium(premium))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                premium,
                $"Premium must be greater than 0, at most {PolicyRules.MaxPremium}" +
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
}
