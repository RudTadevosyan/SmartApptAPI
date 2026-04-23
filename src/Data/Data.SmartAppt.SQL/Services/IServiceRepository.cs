using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface IServiceRepository
    {
        Task<int> CreateAsync(ServiceEntity service, CancellationToken ct = default);
        Task<ServiceEntity?> GetByIdAsync(int serviceId, CancellationToken ct = default);
        Task<IEnumerable<ServiceEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<IEnumerable<ServiceEntity>> GetByBusinessIdAsync(int businessId, CancellationToken ct = default);
        Task UpdateAsync(ServiceEntity service, CancellationToken ct = default);
        Task DeleteAsync(int serviceId, CancellationToken ct = default);
        Task DeactivateAsync(int serviceId, CancellationToken ct = default);
        Task ActivateAsync(int serviceId, CancellationToken ct = default);
    }
}