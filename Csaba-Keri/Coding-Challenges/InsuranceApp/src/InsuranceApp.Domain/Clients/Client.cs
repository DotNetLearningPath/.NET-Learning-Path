using static InsuranceApp.Domain.Common.Validation.DomainValueNormalizer;

namespace InsuranceApp.Domain.Clients;

public class Client
{
    public const int MaxNameLength = 200;
    public const int MaxIdentificationNumberLength = 50;
    public const int MaxEmailLength = 256;
    public const int MaxPhoneLength = 30;
    public const int MaxPrimaryAddressLength = 500;

    public Guid Id { get; }
    public ClientType Type { get; }
    public string IdentificationNumber { get; }

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string? PrimaryAddress { get; private set; }

    public Client(
        Guid id,
        ClientType type,
        string identificationNumber,
        string name,
        string email,
        string phone,
        string? primaryAddress = null
    )
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Client identifier must not be empty.",
                nameof(id)
            );
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Client type is invalid."
            );
        }

        Id = id;
        Type = type;

        IdentificationNumber = NormalizeRequired(
            identificationNumber,
            MaxIdentificationNumberLength,
            nameof(identificationNumber)
        );

        Name = NormalizeRequired(name, MaxNameLength, nameof(name));
        Email = NormalizeEmail(email, MaxEmailLength, nameof(email));
        Phone = NormalizeRequired(phone, MaxPhoneLength, nameof(phone));
        PrimaryAddress = NormalizePrimaryAddress(primaryAddress);
    }

    public void UpdateDetails(
        string name,
        string email,
        string phone,
        string? primaryAddress
    )
    {
        var normalizedName = NormalizeRequired(name, MaxNameLength, nameof(name));
        var normalizedEmail = NormalizeEmail(email, MaxEmailLength, nameof(email));
        var normalizedPhone = NormalizeRequired(phone, MaxPhoneLength, nameof(phone));
        var normalizedAddress = NormalizePrimaryAddress(primaryAddress);

        Name = normalizedName;
        Email = normalizedEmail;
        Phone = normalizedPhone;
        PrimaryAddress = normalizedAddress;
    }

    private static string? NormalizePrimaryAddress(string? primaryAddress)
    {
        return string.IsNullOrWhiteSpace(primaryAddress)
            ? null
            : NormalizeRequired(primaryAddress, MaxPrimaryAddressLength, nameof(primaryAddress));
    }
}
