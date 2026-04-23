using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface IHolidayRepository
    {
        Task<IEnumerable<HolidayEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<HolidayEntity?> GetByBusinessIdAsync(int businessId, DateTime date, CancellationToken ct = default);
        Task<HolidayEntity?> GetByIdAsync(int holidayId, CancellationToken ct = default);
        Task<int> CreateAsync(HolidayEntity entity, CancellationToken ct = default);
        Task UpdateAsync(HolidayEntity entity, CancellationToken ct = default);
        Task DeleteAsync(int holidayId, CancellationToken ct = default);
        Task<List<HolidayEntity>> GetAllByMonthAsync(int businessId, int year, int month, CancellationToken ct = default);
    }
}