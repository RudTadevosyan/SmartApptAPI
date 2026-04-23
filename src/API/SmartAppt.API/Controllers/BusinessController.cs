using System.Security.Claims;
using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.BusinessModels;
using Business.SmartAppt.Models.HolidayModels;
using Business.SmartAppt.Models.HoursModels;
using Business.SmartAppt.Models.ServiceModels;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers
{
    [ApiController]
    [Route("api/businesses")]
    [Authorize(Roles = "BusinessOwner")] 
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        private Guid GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            
            var result = await _businessService.CreateBusinessAsync(userId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetMyBusiness(int businessId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.GetMyBusinessAsync(userId, businessId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}")]
        public async Task<IActionResult> UpdateBusiness(int businessId, [FromBody] UpdateBusinessModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.UpdateBusinessAsync(userId, businessId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}")]
        public async Task<IActionResult> DeleteBusiness(int businessId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.DeleteBusinessAsync(userId, businessId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/services")]
        public async Task<IActionResult> AddService(int businessId, [FromBody] CreateServiceModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.AddServiceAsync(userId, businessId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/services")]
        public async Task<IActionResult> GetMyServices(int businessId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.GetMyServicesAsync(userId, businessId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}/services/{serviceId}")]
        public async Task<IActionResult> UpdateService(int businessId, int serviceId, [FromBody] UpdateServiceModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.UpdateServiceAsync(userId, businessId, serviceId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/services/{serviceId}")]
        public async Task<IActionResult> DeleteService(int businessId, int serviceId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.DeleteServiceAsync(userId, businessId, serviceId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/services/{serviceId}/deactivate")]
        public async Task<IActionResult> DeactivateService(int businessId, int serviceId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.DeactivateServiceAsync(userId, businessId, serviceId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/services/{serviceId}/activate")]
        public async Task<IActionResult> ActivateService(int businessId, int serviceId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.ActivateServiceAsync(userId, businessId, serviceId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/hours")]
        public async Task<IActionResult> AddOpeningHours(int businessId, [FromBody] CreateHoursModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.AddOpeningHoursAsync(userId, businessId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/hours")]
        public async Task<IActionResult> GetMyOpeningHours(int businessId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.GetMyOpeningHoursAsync(userId, businessId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}/hours/{dow}")]
        public async Task<IActionResult> UpdateOpeningHours(int businessId, byte dow, [FromBody] UpdateHoursModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.UpdateOpeningHoursAsync(userId, businessId, dow, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/hours/{dow}")]
        public async Task<IActionResult> DeleteOpeningHours(int businessId, byte dow, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.DeleteOpeningHoursAsync(userId, businessId, dow, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/holidays")]
        public async Task<IActionResult> AddHoliday(int businessId, [FromBody] CreateHolidayModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.AddHolidayAsync(userId, businessId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/holidays/{holidayId}")]
        public async Task<IActionResult> DeleteHoliday(int businessId, int holidayId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.DeleteHolidayAsync(userId, businessId, holidayId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/bookings/confirmed")]
        public async Task<IActionResult> GetCurrentBookings(int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _businessService.GetCurrentActiveBookings(userId, businessId, pageNumber, pageSize, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/bookings/{year}/{month}/{day}")]
        public async Task<IActionResult> GetDailyBookings(int businessId, int day, int month, int year, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var date = new DateTime(year, month, day);

            var result = await _businessService.GetDailyBookingsAsync(
                userId, businessId, date, pageNumber, pageSize, ct);

            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/bookings/{bookingId}/confirm")]
        public async Task<IActionResult> ConfirmBooking(int businessId, int bookingId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.ConfirmBookingAsync(userId, businessId, bookingId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/bookings/{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int businessId, int bookingId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _businessService.CancelBookingAsync(userId, businessId, bookingId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/bookings")]
        public async Task<IActionResult> GetBookings(int businessId, [FromQuery] BookingRequest request, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _businessService.GetAllBookingsAsync(userId, businessId, request, pageNumber, pageSize, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/customers")]
        public async Task<IActionResult> GetBusinessCustomers(int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _businessService.GetBusinessCustomers(userId, businessId, pageNumber, pageSize, ct);
            return StatusCode(result.HttpStatusCode, result);
        }
    }
}