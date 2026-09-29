using Domain.Enums;

namespace Domain.Entities;

public class Broker
{
    public Guid Id { get; private set; }
    public string BrokerCode { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public BrokerStatus Status { get; private set; }
    public decimal? CommissionPercentage { get; private set; }

    private Broker()
    {
        BrokerCode = null!;
        Name = null!;
        Email = null!;
        Phone = null!;
    }

    public Broker(string brokerCode, string name, string email, string phone,
        BrokerStatus status = BrokerStatus.Active, decimal? commissionPercentage = null)
    {
        if (string.IsNullOrWhiteSpace(brokerCode))
        {
            throw new ArgumentException("Broker code is required.", nameof(brokerCode));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Broker name is required.", nameof(name));
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Broker email is required.", nameof(email));
        }
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Broker phone is required.", nameof(phone));
        }
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException("Broker status is not valid.", nameof(status));
        }
        if (commissionPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(commissionPercentage));
        }

        Id = Guid.NewGuid();
        BrokerCode = brokerCode;
        Name = name;
        Email = email;
        Phone = phone;
        Status = status;
        CommissionPercentage = commissionPercentage;
    }
}
