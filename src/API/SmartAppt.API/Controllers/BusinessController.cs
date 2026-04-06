using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.BusinessModels;
using Business.SmartAppt.Models.HolidayModels;
using Business.SmartAppt.Models.HoursModels;
using Business.SmartAppt.Models.ServiceModels;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessModel model)
        {
            var result = await _businessService.CreateBusinessAsync(model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetMyBusiness(int businessId)
        {
            var result = await _businessService.GetMyBusinessAsync(businessId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}")]
        public async Task<IActionResult> UpdateBusiness(int businessId, [FromBody] UpdateBusinessModel model)
        {
            var result = await _businessService.UpdateBusinessAsync(businessId, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}")]
        public async Task<IActionResult> DeleteBusiness(int businessId)
        {
            var result = await _businessService.DeleteBusinessAsync(businessId);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [HttpPost("{businessId}/services")]
        public async Task<IActionResult> AddService(int businessId, [FromBody] CreateServiceModel model)
        {
            var result = await _businessService.AddServiceAsync(businessId, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/services")]
        public async Task<IActionResult> GetMyServices(int businessId)
        {
            var result = await _businessService.GetMyServicesAsync(businessId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}/services/{serviceId}")]
        public async Task<IActionResult> UpdateService(
            int businessId,
            int serviceId,
            [FromBody] UpdateServiceModel model)
        {
            var result = await _businessService.UpdateServiceAsync(businessId, serviceId, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/services/{serviceId}")]
        public async Task<IActionResult> DeleteService(int businessId, int serviceId)
        {
            var result = await _businessService.DeleteServiceAsync(businessId, serviceId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/services/{serviceId}/deactivate")]
        public async Task<IActionResult> DeactivateService(int businessId, int serviceId)
        {
            var result = await _businessService.DeactivateServiceAsync(businessId, serviceId);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [HttpPost("{businessId}/services/{serviceId}/activate")]
        public async Task<IActionResult> ActivateService(int businessId, int serviceId)
        {
            var result = await _businessService.ActivateServiceAsync(businessId, serviceId);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [HttpPost("{businessId}/hours")]
        public async Task<IActionResult> AddOpeningHours(int businessId, [FromBody] CreateHoursModel model)
        {
            var result = await _businessService.AddOpeningHoursAsync(businessId, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/hours")]
        public async Task<IActionResult> GetMyOpeningHours(int businessId)
        {
            var result = await _businessService.GetMyOpeningHoursAsync(businessId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPut("{businessId}/hours/{dow}")]
        public async Task<IActionResult> UpdateOpeningHours(
            int businessId,
            byte dow,
            [FromBody] UpdateHoursModel model)
        {
            var result = await _businessService.UpdateOpeningHoursAsync(businessId, dow, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/hours/{dow}")]
        public async Task<IActionResult> DeleteOpeningHours(int businessId, byte dow)
        {
            var result = await _businessService.DeleteOpeningHoursAsync(businessId, dow);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [HttpPost("{businessId}/holidays")]
        public async Task<IActionResult> AddHoliday(int businessId, [FromBody] CreateHolidayModel model)
        {
            var result = await _businessService.AddHolidayAsync(businessId, model);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpDelete("{businessId}/holidays/{holidayId}")]
        public async Task<IActionResult> DeleteHoliday(int businessId, int holidayId)
        {
            var result = await _businessService.DeleteHolidayAsync(businessId, holidayId);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [HttpGet("{businessId}/bookings/confirmed")]
        public async Task<IActionResult> GetCurrentBookings(
            int businessId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _businessService.GetCurrentActiveBookings(businessId, pageNumber, pageSize);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/bookings/{year}/{month}/{day}")]
        public async Task<IActionResult> GetDailyBookings(
            int businessId,
            int day,
            int month,
            int year,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            DateTime date = new DateTime(year, month, day);
            var result = await _businessService.GetDailyBookingsAsync(businessId, date, pageNumber, pageSize);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/bookings/{bookingId}/confirm")]
        public async Task<IActionResult> ConfirmBooking(int businessId, int bookingId)
        {
            var result = await _businessService.ConfirmBookingAsync(businessId, bookingId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpPost("{businessId}/bookings/{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int businessId, int bookingId)
        {
            var result = await _businessService.CancelBookingAsync(businessId, bookingId);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/bookings/")]
        public async Task<IActionResult> GetBookings(
            int businessId,
            [FromQuery] BookingRequest request,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _businessService.GetAllBookingsAsync(businessId, request, pageNumber, pageSize);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("{businessId}/customers/")]
        public async Task<IActionResult> GetBusinessCustomers(int businessId, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _businessService.GetBusinessCustomers(businessId, pageNumber, pageSize);
            return StatusCode(result.HttpStatusCode, result);
        }
    }
}
