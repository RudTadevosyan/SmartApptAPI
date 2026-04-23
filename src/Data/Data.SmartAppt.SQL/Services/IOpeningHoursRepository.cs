using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface IOpeningHoursRepository
    {
        Task<IEnumerable<OpeningHoursEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<OpeningHoursEntity?> GetByBusinessIdAndDowAsync(int businessId, byte dayOfWeek, CancellationToken ct = default);
        Task<IEnumerable<OpeningHoursEntity>> GetByBusinessIdAsync(int businessId, CancellationToken ct = default);
        Task<int> CreateAsync(OpeningHoursEntity entity, CancellationToken ct = default);
        Task UpdateAsync(OpeningHoursEntity entity, CancellationToken ct = default);
        Task DeleteAsync(int hoursId, CancellationToken ct = default);
    }
}