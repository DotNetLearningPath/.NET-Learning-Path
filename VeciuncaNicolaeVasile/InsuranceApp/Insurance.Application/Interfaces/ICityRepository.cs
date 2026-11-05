namespace Insurance.Application.Interfaces
{
    public interface ICityRepository
    {
        Task<bool> ExistsAsync(Guid cityId, CancellationToken cancellationToken);
    }
}
