using Insurance.Application.DTO.Fees;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Application.DTO.Fees;

public sealed class FeeConfigurationDtoTest
{
    private readonly DateTime _effectiveFrom;
    private readonly DateTime _effectiveTo;

    public FeeConfigurationDtoTest()
    {
        _effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _effectiveTo = _effectiveFrom.AddYears(1);
    }

    [Theory]
    [InlineData(FeeType.BrokerCommission)]
    [InlineData(FeeType.RiskAdjustment)]
    [InlineData(FeeType.AdminFee)]
    public void FeeConfigurationDto_PreservesProvidedValues(FeeType type)
    {
        var id = Guid.NewGuid();
        var dto = new FeeConfigurationDto
        {
            Id = id,
            Name = "Annual fee",
            Type = type,
            Percentage = 4.25m,
            EffectiveFrom = _effectiveFrom,
            EffectiveTo = _effectiveTo,
            IsActive = false
        };

        Assert.Equal(id, dto.Id);
        Assert.Equal("Annual fee", dto.Name);
        Assert.Equal(type, dto.Type);
        Assert.Equal(4.25m, dto.Percentage);
        Assert.Equal(_effectiveFrom, dto.EffectiveFrom);
        Assert.Equal(_effectiveTo, dto.EffectiveTo);
        Assert.False(dto.IsActive);
    }

    [Fact]
    public void SaveFeeConfigurationRequest_UsesActiveByDefault()
    {
        var request = new SaveFeeConfigurationRequest();

        Assert.True(request.IsActive);
    }

    [Theory]
    [InlineData(FeeType.BrokerCommission)]
    [InlineData(FeeType.RiskAdjustment)]
    [InlineData(FeeType.AdminFee)]
    public void SaveFeeConfigurationRequest_PreservesProvidedValues(FeeType type)
    {
        var request = new SaveFeeConfigurationRequest
        {
            Name = "Quarterly fee",
            Type = type,
            Percentage = 2.75m,
            EffectiveFrom = _effectiveFrom,
            EffectiveTo = _effectiveTo,
            IsActive = false
        };

        Assert.Equal("Quarterly fee", request.Name);
        Assert.Equal(type, request.Type);
        Assert.Equal(2.75m, request.Percentage);
        Assert.Equal(_effectiveFrom, request.EffectiveFrom);
        Assert.Equal(_effectiveTo, request.EffectiveTo);
        Assert.False(request.IsActive);
    }
}
