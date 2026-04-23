using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services
{
    public interface IBookingRepository
    {
        Task<IEnumerable<BookingEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<IEnumerable<BookingEntity>> GetAllSpecAsync(BookingFilter filter, CancellationToken ct = default); 
        Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken ct = default);
        Task<int> CreateAsync(BookingEntity entity, CancellationToken ct = default);
        Task UpdateAsync(BookingEntity entity, CancellationToken ct = default);
        Task DeleteAsync(int bookingId, CancellationToken ct = default);
        Task CancelAsync(int bookingId, CancellationToken ct = default);
        Task ChangeBookingStatusAsync(int bookingId, string status, CancellationToken ct = default);
        Task<IEnumerable<BookingEntity>> GetBookingsByRangeAsync(int businessId, DateTime from, DateTime to, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Dictionary<DateOnly, int>> GetBookingsCountByBusinessAndRangeAsync(int businessId, int serviceId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
    }
}
