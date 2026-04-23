using Business.SmartAppt.Models;
using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.BusinessModels;
using Business.SmartAppt.Models.HolidayModels;
using Business.SmartAppt.Models.HoursModels;
using Business.SmartAppt.Models.ServiceModels;

namespace Business.SmartAppt.Services
{
    public interface IBusinessService
    {
        // Business
        Task<BaseResponse<BusinessModel>> CreateBusinessAsync(Guid ownerUserId, CreateBusinessModel business, CancellationToken ct = default);
        Task<BaseResponse<bool>> UpdateBusinessAsync(Guid ownerUserId, int businessId, UpdateBusinessModel business, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteBusinessAsync(Guid ownerUserId, int businessId, CancellationToken ct = default);
        Task<BaseResponse<BusinessModel>> GetMyBusinessAsync(Guid ownerUserId, int businessId, CancellationToken ct = default);
        Task<BaseResponse<IEnumerable<CustomerModel>>> GetBusinessCustomers(Guid ownerUserId, int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

        // Service
        Task<BaseResponse<ServiceModel>> AddServiceAsync(Guid ownerUserId, int businessId, CreateServiceModel service, CancellationToken ct = default);
        Task<BaseResponse<bool>> UpdateServiceAsync(Guid ownerUserId, int businessId, int serviceId, UpdateServiceModel service, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default);
        Task<BaseResponse<bool>> ActivateServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeactivateServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default);
        Task<BaseResponse<IEnumerable<ServiceModel>>> GetMyServicesAsync(Guid ownerUserId, int businessId, CancellationToken ct = default);

        // OpeningHours
        Task<BaseResponse<HoursModel>> AddOpeningHoursAsync(Guid ownerUserId, int businessId, CreateHoursModel hours, CancellationToken ct = default);
        Task<BaseResponse<bool>> UpdateOpeningHoursAsync(Guid ownerUserId, int businessId, byte dow, UpdateHoursModel hours, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteOpeningHoursAsync(Guid ownerUserId, int businessId, byte dow, CancellationToken ct = default);
        Task<BaseResponse<IEnumerable<HoursModel>>> GetMyOpeningHoursAsync(Guid ownerUserId, int businessId, CancellationToken ct = default);

        // Holiday
        Task<BaseResponse<HolidayModel>> AddHolidayAsync(Guid ownerUserId, int businessId, CreateHolidayModel holiday, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeleteHolidayAsync(Guid ownerUserId, int businessId, int holidayId, CancellationToken ct = default);

        // Bookings (business side)
        Task<BaseResponse<IEnumerable<BookingModel>>> GetCurrentActiveBookings(Guid ownerUserId, int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<BaseResponse<IEnumerable<BookingModel>>> GetAllBookingsAsync(Guid ownerUserId, int businessId, BookingRequest request, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<BaseResponse<IEnumerable<BookingModel>>> GetDailyBookingsAsync(Guid ownerUserId, int businessId, DateTime date, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<BaseResponse<bool>> ConfirmBookingAsync(Guid ownerUserId, int businessId, int bookingId, CancellationToken ct = default);
        Task<BaseResponse<bool>> CancelBookingAsync(Guid ownerUserId, int businessId, int bookingId, CancellationToken ct = default);
    }
}