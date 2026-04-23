using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<IEnumerable<CustomerEntity>> GetByBusinessIdAsync(int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<CustomerEntity?> GetByIdAsync(int customerId, CancellationToken ct = default);
        Task<int> CreateAsync(CustomerEntity entity, CancellationToken ct = default);
        Task UpdateAsync(CustomerEntity customerEntity, CancellationToken ct = default);
        Task DeleteAsync(int customerId, CancellationToken ct = default);
        Task<CustomerEntity?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}