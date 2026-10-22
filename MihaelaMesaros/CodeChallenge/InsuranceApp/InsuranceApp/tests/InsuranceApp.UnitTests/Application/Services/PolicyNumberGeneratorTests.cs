using InsuranceApp.Application.Services;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class PolicyNumberGeneratorTests
{
    private readonly PolicyNumberGeneratorService _service;

    public PolicyNumberGeneratorTests() 
    {
        _service = new PolicyNumberGeneratorService();
    }
    
    [Fact]
    public void PolicyNumberGenerator_ReturnsValidFormat()
    {
        //Arrange
        var expectedYear = DateTime.UtcNow.Year;

        //Act
        var generatedPolicyNumber = _service.GeneratePolicyNumber();

        //Assert
        Assert.StartsWith($"{expectedYear}-", generatedPolicyNumber);
        Assert.Matches(@"^\d{4}-[A-Z0-9]{10}$", generatedPolicyNumber);
    }
}
