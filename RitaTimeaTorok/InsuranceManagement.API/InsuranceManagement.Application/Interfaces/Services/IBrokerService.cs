using InsuranceManagement.Application.DTOs.Broker;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IBrokerService
    {
        Task<PagedResult<BrokerDto>> GetAllBrokersAsync(int pageNumber, int pageSize);
        Task<BrokerDto> GetBrokerByIdAsync(Guid id);
        Task<BrokerDto> CreateBrokerAsync(CreateBrokerDto request);
        Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerDto request);
    }
}
