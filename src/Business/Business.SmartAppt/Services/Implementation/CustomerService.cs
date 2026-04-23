using Business.SmartAppt.Models;
using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.CustomerModels;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using CustomerModel = Business.SmartAppt.Models.CustomerModel;

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

        public virtual async Task<BaseResponse<bool>> CancelBookingAsync(Guid userId, int bookingId, CancellationToken ct = default)
        {
            try
            {
                if (Guid.Empty == userId)
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 400, Message = "Invalid user Id" 
                    };
                
                var booking = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (booking == null)
                {
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 404, 
                        Message = $"Booking with {bookingId} id not found"
                    };
                }
                
                var customer = await CustomerRepository.GetByIdAsync(booking.CustomerId, ct);
                if (customer == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Customer {booking.CustomerId} not found"};

                if (customer.UserId != userId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };

                await BookingRepository.CancelAsync(bookingId, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    HttpStatusCode = 500, 
                    Message = ex.Message
                };
            }

        }

        public virtual async Task<BaseResponse<BookingModel>> CreateBookingAsync(Guid userId, CreateBookingModel booking, CancellationToken ct = default)
        {
            try
            {
                
                if (Guid.Empty == userId)
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 400, Message = "Invalid user Id" 
                    };
                
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
                var customer = await CustomerRepository.GetByUserIdAsync(userId, ct);
                if (customer == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Customer not found"};

                var business = await BusinessRepository.GetByIdAsync(booking.BusinessId, ct);
                if (business == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Business with ID {booking.BusinessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(booking.ServiceId, ct);
                if (service == null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 404, Message = $"Service with ID {booking.ServiceId} not found" };

                // check 
                if (service.BusinessId != business.BusinessId)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Business doesnt have that service" };

                if (!service.IsActive)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Service is not active" };
                
                if (customer.BusinessId != business.BusinessId) 
                    return new BaseResponse<BookingModel> { HttpStatusCode = 403, Message = "Forbidden" };

                // determine the ending time
                var endAtUtc = booking.StartAtUtc.AddMinutes(service.DurationMin);

                // check date for holidays
                var holiday = await HolidayRepository.GetByBusinessIdAsync(booking.BusinessId, booking.StartAtUtc.Date, ct);
                if (holiday != null)
                    return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = $"It's a holiday" };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  // monday = 1, sunday = 7

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(booking.BusinessId, dow, ct);
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
                    CustomerId = customer.CustomerId,
                    Date = booking.StartAtUtc.Date
                }, ct);

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
                }, ct);
                
                foreach (var b in existing)
                {
                    if (b.StartAtUtc == booking.StartAtUtc)
                        return new BaseResponse<BookingModel> { HttpStatusCode = 400, Message = "Time overlaps with an existing booking." };
                }

                var entity = new BookingEntity
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customer.CustomerId,
                    Status = booking.Status,
                    Notes = booking.Notes,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = endAtUtc
                };

                int id = await BookingRepository.CreateAsync(entity, ct);

                BookingModel bm = new BookingModel
                {
                    BookingId = id,
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customer.CustomerId,
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
        public virtual async Task<BaseResponse<BookingModel>> GetBookingByIdAsync(Guid userId, int bookingId, CancellationToken ct = default)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 400,
                        Message = "Invalid user Id"
                    };

                var booking = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (booking == null)
                {
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Booking with ID {bookingId} not found"
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(booking.BusinessId, ct);
                if (business == null)
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Customer {booking.CustomerId} not found"
                    };
                
                var customer = await CustomerRepository.GetByIdAsync(booking.CustomerId, ct);
                if (customer == null)
                {
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Customer {booking.CustomerId} not found"
                    };
                }

                // let and customer and business owner to access 
                if (customer.UserId != userId && business.OwnerUserId != userId)
                {
                    return new BaseResponse<BookingModel>
                    {
                        HttpStatusCode = 403,
                        Message = "Forbidden"
                    };
                }

                var result = new BookingModel
                {
                    BookingId = booking.BookingId,
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = booking.CustomerId,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = booking.EndAtUtc,
                    Status = booking.Status,
                    Notes = booking.Notes
                };

                return new BaseResponse<BookingModel>
                {
                    Data = result,
                    HttpStatusCode = 200,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingModel>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteBookingAsync(Guid userId, int bookingId, CancellationToken ct = default)
        {
            try
            {
                if (Guid.Empty == userId)
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 400, Message = "Invalid user Id" 
                    };

                
                var booking = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (booking == null)
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 404, 
                        Message = $"Booking with {bookingId} id not found"
                    };
                }
                
                var customer = await CustomerRepository.GetByIdAsync(booking.CustomerId, ct);
                if (customer == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Customer {booking.CustomerId} not found"};

                if (customer.UserId != userId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                await BookingRepository.DeleteAsync(bookingId, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> {HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<DailySlotsModel>> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date, CancellationToken ct = default)
        {
            try
            {
                if (date <= DateOnly.FromDateTime(DateTime.UtcNow))
                    return new BaseResponse<DailySlotsModel>
                    {
                        HttpStatusCode = 400, Data = new DailySlotsModel()
                        {
                            Date = date
                        }
                    };
                
                // checking basic requirements
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 404, Message = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (service == null)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 404, Message = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new BaseResponse<DailySlotsModel> { HttpStatusCode = 400, Message = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new BaseResponse<DailySlotsModel>() { HttpStatusCode = 400, Message = "Service is not active" };
                
                var holiday = await HolidayRepository.GetByBusinessIdAsync(businessId, new DateTime(date.Year, date.Month, date.Day), ct);
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

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow, ct);
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
                }, ct);


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

        public virtual async Task<BaseResponse<bool>> UpdateCustomerAsync(Guid userId, int customerId, UpdateCustomerModel updateCustomer, CancellationToken ct = default)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 400,
                        Message = "Invalid user Id"
                    };

                var customer = await CustomerRepository.GetByIdAsync(customerId, ct);
                if (customer == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = "Customer not found" };

                if (customer.UserId != userId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var entity = new CustomerEntity
                {
                    CustomerId = customer.CustomerId,
                    BusinessId = customer.BusinessId,
                    FullName = updateCustomer.FullName ?? customer.FullName,
                    Email = updateCustomer.Email ?? customer.Email,
                    Phone = updateCustomer.Phone ?? customer.Phone, 
                    UserId = userId
                };


                await CustomerRepository.UpdateAsync(entity, ct);

                return new BaseResponse<bool>
                {
                    Data = true,
                    HttpStatusCode = 200,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteCustomerAsync(Guid userId, int customerId, CancellationToken ct = default)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 400,
                        Message = "Invalid user Id"
                    };

                var customer = await CustomerRepository.GetByIdAsync(customerId, ct);
                if (customer == null)
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 404,
                        Message = $"Customer with ID {customerId} not found"
                    };

                if (customer.UserId != userId)
                    return new BaseResponse<bool>
                    {
                        HttpStatusCode = 403,
                        Message = "Forbidden"
                    };

                await CustomerRepository.DeleteAsync(customerId, ct);

                return new BaseResponse<bool>
                {
                    Data = true,
                    HttpStatusCode = 200,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<CustomerModel>> GetCustomerByIdAsync(Guid userId, int customerId, CancellationToken ct = default)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 400,
                        Message = "Invalid user Id"
                    };

                var customer = await CustomerRepository.GetByIdAsync(customerId, ct);
                if (customer == null)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Customer with ID {customerId} not found"
                    };
                
                var business = await BusinessRepository.GetByIdAsync(customer.BusinessId, ct);
                if (business == null)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Customer with ID {customerId} not found"
                    };
                    

                if (customer.UserId != userId && business.OwnerUserId != userId)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 403,
                        Message = "Forbidden"
                    };

                var result = new CustomerModel
                {
                    CustomerId = customer.CustomerId,
                    BusinessId = customer.BusinessId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone
                };

                return new BaseResponse<CustomerModel>
                {
                    Data = result,
                    HttpStatusCode = 200,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<CustomerModel>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<CustomerModel>> CreateCustomerAsync(Guid userId, CreateCustomerModel customer, CancellationToken ct = default)
        {
            try
            {
                if (userId == Guid.Empty)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 400,
                        Message = "Invalid user Id"
                    };

                var business = await BusinessRepository.GetByIdAsync(customer.BusinessId, ct);
                if (business == null)
                    return new BaseResponse<CustomerModel>
                    {
                        HttpStatusCode = 404,
                        Message = $"Business {customer.BusinessId} not found"
                    };

                var entity = new CustomerEntity
                {
                    BusinessId = customer.BusinessId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    UserId = userId,
                };

                int id = await CustomerRepository.CreateAsync(entity, ct);

                var result = new CustomerModel
                {
                    CustomerId = id,
                    BusinessId = customer.BusinessId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone
                };

                return new BaseResponse<CustomerModel>
                {
                    Data = result,
                    HttpStatusCode = 200,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<CustomerModel>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public virtual async Task<BaseResponse<CalendarModel>> GetMonthlyCalendar(int businessId, int serviceId, int month, int year, CancellationToken ct = default)
        {
            try
            { 
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 404, Message = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (service == null)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 404, Message = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 400, Message = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new BaseResponse<CalendarModel> { HttpStatusCode = 400, Message = "Service is not active" };

                var monthHolidays = await HolidayRepository.GetAllByMonthAsync(businessId, year, month, ct);
                var holidayDates = new HashSet<DateTime>(monthHolidays.Select(h => h.HolidayDate.Date));
                var weekHours = await OpeningHoursRepository.GetByBusinessIdAsync(businessId, ct);


                int daysInMonth = DateTime.DaysInMonth(year, month);
                var startDate = new DateOnly(year, month, 1);
                var endDate = startDate.AddDays(daysInMonth);
                var bookings = await BookingRepository.GetBookingsCountByBusinessAndRangeAsync(businessId, serviceId, startDate, endDate, ct);
                
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

        public virtual async Task<BaseResponse<IEnumerable<BookingModel>>> GetMyBookingsAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            try
            {
                if (Guid.Empty == userId)
                    return new BaseResponse<IEnumerable<BookingModel>>
                    {
                        HttpStatusCode = 400, Message = "Invalid user Id" 
                    };
                
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;

                var customer = await CustomerRepository.GetByUserIdAsync(userId, ct);
                if (customer == null)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 404, Message = $"Customer not found"};
                
                
                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    CustomerId = customer.CustomerId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }, ct);
                
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

        public virtual async Task<BaseResponse<bool>> UpdateBookingAsync(Guid userId, int bookingId, UpdateBookingModel booking, CancellationToken ct = default)
        {
            try
            {
                if (Guid.Empty == userId)
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        HttpStatusCode = 400, Message = "Invalid user Id" 
                    };

                
                // check the date 
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Cant book for previous date"};
                }
                
                // Round the milliseconds
                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);
                
                var existing = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = $"Booking with ID {bookingId} not found" };
                
                var customer = await CustomerRepository.GetByIdAsync(existing.CustomerId, ct);
                if (customer == null)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = $"Customer {existing.CustomerId} not found"};

                if (customer.UserId != userId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                // new booking
                var proposedBooking = new CreateBookingModel
                {
                    BusinessId = existing.BusinessId,
                    ServiceId = existing.ServiceId,
                    StartAtUtc = booking.StartAtUtc,
                    Notes = booking.Notes ?? existing.Notes,
                };

                var service = await ServiceRepository.GetByIdAsync(existing.ServiceId, ct);
                if (service == null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Service not found for this booking" };

                // determine the ending time
                var endAtUtc = proposedBooking.StartAtUtc.AddMinutes(service.DurationMin);

                // check holidays for new booking date
                var holiday = await HolidayRepository.GetByBusinessIdAsync(proposedBooking.BusinessId, proposedBooking.StartAtUtc.Date, ct);
                if (holiday != null)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = $"It's a holiday" };

                // look for hours
                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  // monday = 1, sunday = 7
                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(proposedBooking.BusinessId, dow, ct);
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
                }, ct);

                foreach (var b in existingBookings)
                {
                    if (b.BookingId == bookingId) continue; // skip current booking
                    if (b.StartAtUtc == proposedBooking.StartAtUtc)
                        return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Time overlaps with an existing booking." };
                }

                existing.StartAtUtc = proposedBooking.StartAtUtc;
                existing.EndAtUtc = endAtUtc;
                existing.Notes = proposedBooking.Notes;

                await BookingRepository.UpdateAsync(existing, ct);

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
            if (current <= DateTime.UtcNow.Date)
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
