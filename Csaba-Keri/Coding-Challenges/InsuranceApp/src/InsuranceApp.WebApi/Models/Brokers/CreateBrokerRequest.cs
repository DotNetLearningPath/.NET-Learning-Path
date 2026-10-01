using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Brokers;

public record CreateBrokerRequest(
    [Required]
    string Code,

    [Required]
    string Name,
    
    [Required]
    string Email,
    
    [Required]
    string Phone,
    
    [Required]
    BrokerStatusDto? Status
);
