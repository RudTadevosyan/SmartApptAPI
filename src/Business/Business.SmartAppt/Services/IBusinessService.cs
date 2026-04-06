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
        Task<BaseResponse<BusinessModel>> CreateBusinessAsync(CreateBusinessModel business);
        Task<BaseResponse<bool>> UpdateBusinessAsync(int businessId, UpdateBusinessModel business);
        Task<BaseResponse<bool>> DeleteBusinessAsync(int businessId);
        Task<BaseResponse<BusinessModel>> GetMyBusinessAsync(int businessId);
        Task<BaseResponse<IEnumerable<CustomerModel>>> GetBusinessCustomers(int businessId, int pageNumber = 1, int pageSize = 10);

        // Service
        Task<BaseResponse<ServiceModel>> AddServiceAsync(int businessId, CreateServiceModel service);
        Task<BaseResponse<bool>> UpdateServiceAsync(int businessId, int serviceId, UpdateServiceModel service);
        Task<BaseResponse<bool>> DeleteServiceAsync(int businessId, int serviceId);
        Task<BaseResponse<bool>> ActivateServiceAsync(int businessId, int serviceId);
        Task<BaseResponse<bool>> DeactivateServiceAsync(int businessId, int serviceId);
        Task<BaseResponse<IEnumerable<ServiceModel>>> GetMyServicesAsync(int businessId);

        // OpeningHours
        Task<BaseResponse<HoursModel>> AddOpeningHoursAsync(int businessId,CreateHoursModel hours);
        Task<BaseResponse<bool>> UpdateOpeningHoursAsync(int businessId, byte dow, UpdateHoursModel hours);
        Task<BaseResponse<bool>> DeleteOpeningHoursAsync(int businessId, byte dow);
        Task<BaseResponse<IEnumerable<HoursModel>>> GetMyOpeningHoursAsync(int businessId);

        // Holiday
        Task<BaseResponse<HolidayModel>> AddHolidayAsync(int businessId, CreateHolidayModel holiday);
        Task<BaseResponse<bool>> DeleteHolidayAsync(int businessId, int holidayId);
        
        // Bookings 
        Task<BaseResponse<IEnumerable<BookingModel>>> GetCurrentActiveBookings(int businessId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponse<IEnumerable<BookingModel>>> GetAllBookingsAsync(int businessId, BookingRequest request, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponse<IEnumerable<BookingModel>>> GetDailyBookingsAsync(int businessId, DateTime date, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponse<bool>> ConfirmBookingAsync(int businessId, int bookingId);
        Task<BaseResponse<bool>> CancelBookingAsync(int businessId, int bookingId);
    }
}
