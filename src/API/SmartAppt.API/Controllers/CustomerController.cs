using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Business.SmartAppt.Models.CustomerModels;
using Business.SmartAppt.Services.Implementation;

namespace SmartAppt.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        
        private Guid GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
        
        [Authorize(Roles = "BusinessOwner,Customer")]
        [HttpGet("bookings/{bookingId}")]
        public async Task<IActionResult> GetBookingById(int bookingId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.GetBookingByIdAsync(userId, bookingId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.CreateBookingAsync(userId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("bookings/{bookingId}")]
        public async Task<IActionResult> UpdateBooking(int bookingId, [FromBody] UpdateBookingModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.UpdateBookingAsync(userId, bookingId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("bookings/{bookingId}")]
        public async Task<IActionResult> DeleteBooking(int bookingId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.DeleteBookingAsync(userId, bookingId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("bookings/{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.CancelBookingAsync(userId, bookingId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _customerService.GetMyBookingsAsync(userId, pageNumber, pageSize, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("/api/business/{businessId}/services/{serviceId}/calendar/{year}/{month}/{day}/slots")]
        public async Task<IActionResult> GetDailyFreeSlots(int businessId, int serviceId, int year, int month, int day, CancellationToken ct)
        {
            var date = new DateOnly(year, month, day);
            var result = await _customerService.GetDailyFreeSlots(businessId, serviceId, date, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [HttpGet("/api/business/{businessId}/services/{serviceId}/calendar/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyCalendar(int businessId, int serviceId, int year, int month, CancellationToken ct)
        {
            var result = await _customerService.GetMonthlyCalendar(businessId, serviceId, month, year, ct);
            return StatusCode(result.HttpStatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerModel model, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.CreateCustomerAsync(userId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [Authorize(Roles = "Customer")]
        [HttpPut("{customerId}")]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] UpdateCustomerModel model,
            CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.UpdateCustomerAsync(userId, customerId, model, ct);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [Authorize(Roles = "BusinessOwner,Customer")]
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomerById(int customerId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.GetCustomerByIdAsync(userId, customerId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }
        
        [Authorize(Roles = "Customer")]
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteCustomer(int customerId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _customerService.DeleteCustomerAsync(userId, customerId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }
    }
}