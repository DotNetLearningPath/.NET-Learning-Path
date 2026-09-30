using Insurance.Domain.Entities;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeBuildingRepository : IBuildingRepository
{
    public readonly List<Building> Storage = new();
    public Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        Storage.Add(building);
        return Task.CompletedTask;
    }

    public Task<Building?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Storage.FirstOrDefault(b => b.Id == id));
    }

    public Task<PagedResult<Building>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var all = Storage
            .Where(b => b.ClientId == clientId)
            .OrderBy(b => b.Street)
            .ThenBy(b => b.Number)
            .ThenBy(b => b.Id)
            .ToList();
        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);

        return Task.FromResult(new PagedResult<Building>
        {
            Items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    public Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        // in-memory already updated by reference
        return Task.CompletedTask;
    }
}
