using Business.SmartAppt.Models;
using Business.SmartAppt.Models.BookingModels;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;

namespace Business.SmartAppt.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        protected readonly IBookingRepository BookingRepository;
        protected readonly ICustomerRepository CustomerRepository;
        protected readonly IServiceRepository ServiceRepository;
        protected readonly IBusinessRepository BusinessRepository;
        protected readonly IOpeningHoursRepository OpeningHoursRepository;
        protected readonly IHolidayRepository HolidayRepository;

        public CustomerService(IBookingRepository bookingRepository, ICustomerRepository customerRepository, IServiceRepository serviceRepository, IBusinessRepository businessRepository, IOpeningHoursRepository openingHoursRepository, IHolidayRepository holidayRepository)
        {
            BookingRepository = bookingRepository;
            CustomerRepository = customerRepository;
            ServiceRepository = serviceRepository;
            BusinessRepository = businessRepository;
            OpeningHoursRepository = openingHoursRepository;
            HolidayRepository = holidayRepository;
        }

        public virtual async Task<BaseResponse<bool>> CancelBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 404, 
                        Message = $"Booking with {bookingId} id not found"
                    };
                }
                
                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 404, 
                        Message = $"Customer {customerId} not found"
                    };

                if (booking.CustomerId != customerId)
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 400, 
                        Message = "You don't have permissions for this booking"
                    };

                await BookingRepository.CancelAsync(bookingId);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    HttpStatusCode = 500, 
                    Message = ex.Message
                };
            }

        }

        public virtual async Task<BaseResponse<BookingModel>> CreateBookingAsync(int customerId, CreateBookingModel booking)
        {
            try
            {
                // check the date 
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 400, 
                        Message = "Cant book for previous date"
                    };
                }
                
                // Round the milliseconds
                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);
                
                
                // Check business, service, customer
                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Customer with ID {customerId} not found" };

                var business = await BusinessRepository.GetByIdAsync(booking.BusinessId);
                if (business == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Business with ID {booking.BusinessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(booking.ServiceId);
                if (service == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Service with ID {booking.ServiceId} not found" };

                // check 
                if (service.BusinessId != business.BusinessId)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Business doesnt have that service" };

                if (!service.IsActive)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Service is not active" };
                
                if (customer.BusinessId != business.BusinessId) // depends on the logic
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Customer doesn't belong to this business" };

                // determine the ending time
                var endAtUtc = booking.StartAtUtc.AddMinutes(service.DurationMin);

                // check date for holidays
                var holiday = await HolidayRepository.GetByBusinessIdAsync(booking.BusinessId, booking.StartAtUtc.Date);
                if (holiday != null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = $"It's a holiday" };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  // monday = 1, sunday = 7

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(booking.BusinessId, dow);
                if (hours == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "This business has no opening hours for this day." };

                var openAt = booking.StartAtUtc.Date + hours.OpenTime; // Date + Time
                var closeAt = booking.StartAtUtc.Date + hours.CloseTime;

                // Check if the business is open
                if (booking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Business is not open at this time." };


                if ((booking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Booking must start on service-aligned slot" };
                
                
                // check if the customer has same booking as a pending to not allow double booking
                var clientBooking = await BookingRepository.GetAllSpecAsync
                (new BookingFilter
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId, 
                    CustomerId = customerId,
                    Date = booking.StartAtUtc.Date
                });

                int count = clientBooking.Count();
                if (count > 0)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "You have already booking for that day" };


                // Check the Business's services booking for that day
                var existing = await BookingRepository.GetAllSpecAsync
                (new BookingFilter
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    Status = BookingStatus.Confirmed,
                    Date = booking.StartAtUtc.Date
                });
                
                foreach (var b in existing)
                {
                    if (b.StartAtUtc == booking.StartAtUtc)
                        return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Time overlaps with an existing booking." };
                }

                var entity = new BookingEntity
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customerId,
                    Status = booking.Status,
                    Notes = booking.Notes,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = endAtUtc
                };

                int id = await BookingRepository.CreateAsync(entity);

                BookingModel bm = new BookingModel
                {
                    BookingId = id,
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customerId,
                    Notes = booking.Notes,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = endAtUtc,
                    Status = booking.Status,
                };

                return new BaseResponse<BookingModel>
                {
                    Data = bm,
                    HttpStatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingModel> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 404, 
                        Message = $"Booking with {bookingId} id not found"
                    };
                }
                
                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse<bool> {Data = false, HttpStatusCode = 404, Message = $"Customer {customerId} not found"};

                if (booking.CustomerId != customerId)
                    return new BaseResponse<bool> { Data = false, HttpStatusCode = 400, Message = "You don't have permissions for this booking" };
                
                await BookingRepository.DeleteAsync(bookingId);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> {Data = false, HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<DailySlotsModel>> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date)
        {
            try
            {
                // checking basic requirements
                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 404, Message = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 404, Message = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 400, Message = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new BaseResponse<DailySlotsModel>() { HttpStatusCode = 400, Message = "Service is not active" };
                
                var holiday = await HolidayRepository.GetByBusinessIdAsync(businessId, new DateTime(date.Year, date.Month, date.Day));
                if (holiday != null)
                {
                    return new BaseResponse<DailySlotsModel>
                    {
                        Data = new DailySlotsModel
                        {
                            Date = date,
                        },
                        HttpStatusCode = 400
                    };
                }
                
                byte dow = (byte)((((int)date.DayOfWeek) + 6) % 7 + 1);

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow);
                if (hours == null)
                {
                    
                    return new BaseResponse<DailySlotsModel>
                    {
                        Data = new DailySlotsModel
                        {
                            Date = date,
                        },
                        HttpStatusCode = 400
                    };
                }

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    BusinessId = businessId,
                    ServiceId = serviceId,
                    Status = BookingStatus.Confirmed,
                    Date = new DateTime(date.Year, date.Month, date.Day)
                });


                // Checking free slots 
                List<TimeSpan> freeSlots = new List<TimeSpan>();
                TimeSpan slotTime = hours.OpenTime;
                var duration = TimeSpan.FromMinutes(service.DurationMin);

                var bookedSlots = new HashSet<TimeSpan>(
                bookings.Select(b => b.StartAtUtc.TimeOfDay)
                );

                while (slotTime + duration <= hours.CloseTime)
                {
                    if (!bookedSlots.Contains(slotTime))
                        freeSlots.Add(slotTime);

                    slotTime += duration;
                }

                DailySlotsModel dm =  new DailySlotsModel
                {
                    Date = date,
                    FreeSlots = freeSlots,
                };

                return new BaseResponse<DailySlotsModel>
                {
                    Data = dm,
                    HttpStatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<DailySlotsModel>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<CalendarModel>> GetMonthlyCalendar(int businessId, int serviceId, int month, int year)
        {
            try
            { 
                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 404, Message = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 404, Message = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 400, Message = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 400, Message = "Service is not active" };

                var monthHolidays = await HolidayRepository.GetAllByMonthAsync(businessId, year, month);
                var holidayDates = new HashSet<DateTime>(monthHolidays.Select(h => h.HolidayDate.Date));
                var weekHours = await OpeningHoursRepository.GetByBusinessIdAsync(businessId);


                int daysInMonth = DateTime.DaysInMonth(year, month);
                var startDate = new DateOnly(year, month, 1);
                var endDate = startDate.AddDays(daysInMonth);
                var bookings = await BookingRepository.GetBookingsCountByBusinessAndRangeAsync(businessId, serviceId, startDate, endDate);
                
                var monthlyCalendar = new CalendarModel
                {
                    Month = month,
                    Year = year,
                };
                
                for (int d = 1; d <= daysInMonth; d++)
                {
                    var date = new DateTime(year, month, d);
                    var dayAvailability = GetDayAvailability(date, holidayDates, weekHours, service.DurationMin, bookings);
                    
                    monthlyCalendar.Days.Add(dayAvailability);
                }

                return new BaseResponse<CalendarModel>
                {
                    Data = monthlyCalendar,
                    HttpStatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<CalendarModel>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }         

        public virtual async Task<BaseResponse<IEnumerable<BookingModel>>> GetMyBookingsAsync(int customerId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    CustomerId = customerId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
                
                List<BookingModel> bookingsList = new List<BookingModel>();

                var bookingEntities = bookings.ToList();
                foreach (var b in bookingEntities)
                {
                    bookingsList.Add(new BookingModel
                    {
                        BookingId = b.BookingId,
                        BusinessId = b.BusinessId,
                        ServiceId = b.ServiceId,
                        CustomerId = b.CustomerId,
                        StartAtUtc = b.StartAtUtc,
                        EndAtUtc = b.EndAtUtc,
                        Status = b.Status,
                        Notes = b.Notes
                    });
                }

                return new BaseResponse<IEnumerable<BookingModel>>()
                {
                    Data = bookingsList,
                    HttpStatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> UpdateBookingAsync(int customerId, int bookingId, UpdateBookingModel booking)
        {
            try
            {
                // check the date 
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Cant book for previous date"};
                }
                
                // Round the milliseconds
                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);
                
                var existing = await BookingRepository.GetByIdAsync(bookingId);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = $"Booking with ID {bookingId} not found" };
                
                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = $"Customer {customerId} not found"};

                if (existing.CustomerId != customerId)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "You don't have permissions for this booking" };
                
                // new booking
                var proposedBooking = new CreateBookingModel
                {
                    BusinessId = existing.BusinessId,
                    ServiceId = existing.ServiceId,
                    StartAtUtc = booking.StartAtUtc,
                    Notes = booking.Notes ?? existing.Notes,
                };

                var service = await ServiceRepository.GetByIdAsync(existing.ServiceId);
                if (service == null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Service not found for this booking" };

                // determine the ending time
                var endAtUtc = proposedBooking.StartAtUtc.AddMinutes(service.DurationMin);

                // check holidays for new booking date
                var holiday = await HolidayRepository.GetByBusinessIdAsync(proposedBooking.BusinessId, proposedBooking.StartAtUtc.Date);
                if (holiday != null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = $"It's a holiday" };

                // look for hours
                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  // monday = 1, sunday = 7
                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(proposedBooking.BusinessId, dow);
                if (hours == null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "No opening hours for this day." };

                var openAt = proposedBooking.StartAtUtc.Date + hours.OpenTime;
                var closeAt = proposedBooking.StartAtUtc.Date + hours.CloseTime;

                if (proposedBooking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Business is not open at this time." };
                
                if ((proposedBooking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Booking must start on service-aligned slot" };
                
                var existingBookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    BusinessId = proposedBooking.BusinessId,
                    ServiceId = proposedBooking.ServiceId,
                    Status = BookingStatus.Confirmed,
                    Date = proposedBooking.StartAtUtc.Date
                });

                foreach (var b in existingBookings)
                {
                    if (b.BookingId == bookingId) continue; // skip current booking
                    if (b.StartAtUtc == proposedBooking.StartAtUtc)
                        return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Time overlaps with an existing booking." };
                }

                existing.CustomerId = customerId;
                existing.StartAtUtc = proposedBooking.StartAtUtc;
                existing.EndAtUtc = endAtUtc;
                existing.Notes = proposedBooking.Notes;

                await BookingRepository.UpdateAsync(existing);

                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        private DayAvailability GetDayAvailability(DateTime current, HashSet<DateTime> holidays, IEnumerable<OpeningHoursEntity> weekHours, int durationMin, Dictionary<DateOnly, int> bookings)
        {
            // past day
            if (current < DateTime.UtcNow.Date)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

            // holiday
            if (holidays.Contains(current.Date))
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

            // monday=1..sunday=7
            byte dow = (byte)(((int)current.DayOfWeek + 6) % 7 + 1);
            var hours = weekHours.FirstOrDefault(h => h.DayOfWeek == dow);

            if (hours == null)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

            int openMinutes = (int)(hours.CloseTime - hours.OpenTime).TotalMinutes;
            int maxSlots = openMinutes / durationMin;

            bookings.TryGetValue(DateOnly.FromDateTime(current), out int bookedCount);

            return new DayAvailability
            {
                Day = current.Day,
                IsOpen = true,
                HasFreeSlots = bookedCount < maxSlots
            };
        }

        
    }
}
