using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Brokers;

public record UpdateBrokerRequest(
    [Required]
    string Name,
    
    [Required]
    string Email,
    
    [Required]
    string Phone
);
