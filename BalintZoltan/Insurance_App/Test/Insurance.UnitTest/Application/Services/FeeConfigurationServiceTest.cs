using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Fees;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class FeeConfigurationServiceTest
{
    private readonly FakeFeeRepository _repository;
    private readonly FeeConfigurationService _service;
    private readonly DateTime _effectiveFrom;

    public FeeConfigurationServiceTest()
    {
        _repository = new FakeFeeRepository();
        _service = new FeeConfigurationService(_repository);
        _effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(FeeType.BrokerCommission)]
    [InlineData(FeeType.RiskAdjustment)]
    [InlineData(FeeType.AdminFee)]
    public async Task CreateAsync_AddsFeeAndReturnsDto(FeeType type)
    {
        var request = CreateRequest(type);

        var dto = await _service.CreateAsync(request);

        var savedFee = Assert.Single(_repository.Storage);
        Assert.Equal(savedFee.Id, dto.Id);
        Assert.Equal(request.Name, savedFee.Name);
        Assert.Equal(request.Name, dto.Name);
        Assert.Equal(request.Type, dto.Type);
        Assert.Equal(request.Percentage, dto.Percentage);
        Assert.Equal(request.EffectiveFrom, dto.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, dto.EffectiveTo);
        Assert.Equal(request.IsActive, dto.IsActive);
        Assert.Equal(1, _repository.AddCallCount);
    }

    [Fact]
    public async Task ListAsync_ReturnsMappedPageAndPaginationMetadata()
    {
        var firstFee = CreateFee("A fee", FeeType.AdminFee, 2m);
        var secondFee = CreateFee("B fee", FeeType.RiskAdjustment, 3m);
        await _repository.AddFeeConfigurationAsync(firstFee, CancellationToken.None);
        await _repository.AddFeeConfigurationAsync(secondFee, CancellationToken.None);

        var result = await _service.ListAsync(new PaginationRequest
        {
            PageNumber = 2,
            PageSize = 1
        });

        var dto = Assert.Single(result.Items);
        Assert.Equal(secondFee.Id, dto.Id);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_UpdatesFeeAndActiveState(bool isActive)
    {
        var fee = CreateFee("Original fee", FeeType.AdminFee, 2m);
        await _repository.AddFeeConfigurationAsync(fee, CancellationToken.None);
        var request = new SaveFeeConfigurationRequest
        {
            Name = "Updated fee",
            Type = FeeType.BrokerCommission,
            Percentage = 8.5m,
            EffectiveFrom = _effectiveFrom.AddDays(1),
            EffectiveTo = _effectiveFrom.AddYears(1),
            IsActive = isActive
        };

        var dto = await _service.UpdateAsync(fee.Id, request);

        Assert.Equal("Updated fee", fee.Name);
        Assert.Equal(FeeType.BrokerCommission, fee.Type);
        Assert.Equal(8.5m, fee.Percentage);
        Assert.Equal(request.EffectiveFrom, fee.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, fee.EffectiveTo);
        Assert.Equal(isActive, fee.IsActive);
        Assert.Equal(fee.Id, dto.Id);
        Assert.Equal(isActive, dto.IsActive);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundWhenFeeDoesNotExist()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), CreateRequest(FeeType.AdminFee)));

        Assert.Equal("Fee configuration was not found.", exception.Message);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotPersistWhenRequestIsInvalid()
    {
        var fee = CreateFee("Original fee", FeeType.AdminFee, 2m);
        await _repository.AddFeeConfigurationAsync(fee, CancellationToken.None);
        var request = new SaveFeeConfigurationRequest
        {
            Name = "Invalid fee",
            Type = FeeType.AdminFee,
            Percentage = 101m,
            EffectiveFrom = _effectiveFrom,
            IsActive = true
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.UpdateAsync(fee.Id, request));

        Assert.Equal("Original fee", fee.Name);
        Assert.Equal(2m, fee.Percentage);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_DeactivatesExistingFee()
    {
        var fee = CreateFee("Fee to deactivate", FeeType.AdminFee, 2m);
        await _repository.AddFeeConfigurationAsync(fee, CancellationToken.None);

        await _service.DeactivateAsync(fee.Id);

        Assert.False(fee.IsActive);
        Assert.Equal(1, _repository.DeactivateCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_ThrowsNotFoundWhenFeeDoesNotExist()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeactivateAsync(Guid.NewGuid()));

        Assert.Equal("Fee configuration was not found.", exception.Message);
        Assert.Equal(0, _repository.DeactivateCallCount);
    }

    private SaveFeeConfigurationRequest CreateRequest(FeeType type)
    {
        return new SaveFeeConfigurationRequest
        {
            Name = "Service fee",
            Type = type,
            Percentage = 5.5m,
            EffectiveFrom = _effectiveFrom,
            IsActive = true
        };
    }

    private FeeConfiguration CreateFee(string name, FeeType type, decimal percentage)
    {
        return new FeeConfiguration(name, type, percentage, _effectiveFrom);
    }
}
