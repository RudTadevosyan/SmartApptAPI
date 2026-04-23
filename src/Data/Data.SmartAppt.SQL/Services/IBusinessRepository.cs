using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface IBusinessRepository
    {
        Task<int> CreateAsync(BusinessEntity businessData, CancellationToken ct = default);
        Task<BusinessEntity?> GetByIdAsync(int businessId, CancellationToken ct = default);
        Task<BusinessEntity?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default);
        Task<IEnumerable<BusinessEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task UpdateAsync(BusinessEntity businessData, CancellationToken ct = default);
        Task DeleteAsync(int businessId, CancellationToken ct = default);
    }
}