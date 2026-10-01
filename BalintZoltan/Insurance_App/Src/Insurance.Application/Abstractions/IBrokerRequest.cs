namespace Insurance.Application.Abstractions;

public interface IBrokerRequest
{
    string BrokerCode { get; }
    string Name { get; }
    string Email { get; }
    string Phone { get; }
    decimal? CommissionPercentage { get; }
}
