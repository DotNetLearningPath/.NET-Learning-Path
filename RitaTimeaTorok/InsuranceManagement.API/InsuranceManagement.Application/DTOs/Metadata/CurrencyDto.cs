namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class CurrencyDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}
