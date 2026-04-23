using Business.SmartAppt.Models;
using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.CustomerModels;
using CustomerModel = Business.SmartAppt.Models.CustomerModel;

namespace Business.SmartAppt.Services
{
    public interface ICustomerService
    {
        Task<BaseResponse<IEnumerable<BookingModel>>> GetMyBookingsAsync(Guid userId, int pageNumber = 1, int pageSize = 1, CancellationToken ct = default);
        Task<BaseResponse<BookingModel>> CreateBookingAsync(Guid userId, CreateBookingModel booking, CancellationToken ct = default);
        Task<BaseResponse<BookingModel>> GetBookingByIdAsync(Guid userId, int bookingId, CancellationToken ct = default);
        Task<BaseResponse<bool>> UpdateBookingAsync(Guid userId, int bookingId, UpdateBookingModel booking, CancellationToken ct = default);
        Task<BaseResponse<bool>> CancelBookingAsync(Guid userId, int bookingId, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteBookingAsync(Guid userId, int bookingId, CancellationToken ct = default);
        Task<BaseResponse<CalendarModel>> GetMonthlyCalendar(int businessId, int serviceId, int month, int year, CancellationToken ct = default);
        Task<BaseResponse<DailySlotsModel>> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date, CancellationToken ct = default);
        Task<BaseResponse<CustomerModel>> CreateCustomerAsync(Guid userId, CreateCustomerModel customer, CancellationToken ct = default);
        Task<BaseResponse<bool>> UpdateCustomerAsync(Guid userId, int customerId, UpdateCustomerModel updateCustomer, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteCustomerAsync(Guid userId, int customerId, CancellationToken ct = default);
        Task<BaseResponse<CustomerModel>> GetCustomerByIdAsync(Guid userId, int customerId, CancellationToken ct = default);
    }
}