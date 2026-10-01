using static InsuranceApp.Domain.Common.Validation.DomainValueNormalizer;

namespace InsuranceApp.Domain.Brokers;

public class Broker
{
    public const int MaxCodeLength = 50;
    public const int MaxNameLength = 200;
    public const int MaxEmailLength = 256;
    public const int MaxPhoneLength = 30;

    public Guid Id { get; }
    public string Code { get; }

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public BrokerStatus Status { get; private set; }

    public Broker(
        Guid id,
        string code,
        string name,
        string email,
        string phone,
        BrokerStatus status
    )
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Broker identifier must not be empty.",
                nameof(id)
            );
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Broker status is invalid."
            );
        }

        Id = id;
        Code = NormalizeRequired(code, MaxCodeLength, nameof(code)).ToUpperInvariant();
        Name = NormalizeRequired(name, MaxNameLength, nameof(name));
        Email = NormalizeEmail(email, MaxEmailLength, nameof(email));
        Phone = NormalizeRequired(phone, MaxPhoneLength, nameof(phone));
        Status = status;
    }

    public void UpdateDetails(
        string name,
        string email,
        string phone
    )
    {
        var normalizedName = NormalizeRequired(name, MaxNameLength, nameof(name));
        var normalizedEmail = NormalizeEmail(email, MaxEmailLength, nameof(email));
        var normalizedPhone = NormalizeRequired(phone, MaxPhoneLength, nameof(phone));

        Name = normalizedName;
        Email = normalizedEmail;
        Phone = normalizedPhone;
    }

    public void Activate() => Status = BrokerStatus.Active;

    public void Deactivate() => Status = BrokerStatus.Inactive;
}
