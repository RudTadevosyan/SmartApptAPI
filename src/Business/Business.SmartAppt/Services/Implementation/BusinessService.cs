using Business.SmartAppt.Models;
using Business.SmartAppt.Models.BookingModels;
using Business.SmartAppt.Models.BusinessModels;
using Business.SmartAppt.Models.HolidayModels;
using Business.SmartAppt.Models.HoursModels;
using Business.SmartAppt.Models.ServiceModels;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;

namespace Business.SmartAppt.Services.Implementation
{
    public class BusinessService : IBusinessService
    {
        protected readonly IBookingRepository BookingRepository;
        protected readonly IServiceRepository ServiceRepository;
        protected readonly IBusinessRepository BusinessRepository;
        protected readonly IOpeningHoursRepository OpeningHoursRepository;
        protected readonly IHolidayRepository HolidayRepository;
        protected readonly ICustomerRepository CustomerRepository;

        public BusinessService
            (IBookingRepository bookingRepository, IServiceRepository serviceRepository, IBusinessRepository businessRepository,
            IOpeningHoursRepository openingHoursRepository, IHolidayRepository holidayRepository, ICustomerRepository customerRepository)
        {
            BookingRepository = bookingRepository;
            ServiceRepository = serviceRepository;
            BusinessRepository = businessRepository;
            OpeningHoursRepository = openingHoursRepository;
            HolidayRepository = holidayRepository;
            CustomerRepository = customerRepository;
        }


        public virtual async Task<BaseResponse<BusinessModel>> CreateBusinessAsync(Guid ownerUserId, CreateBusinessModel business, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<BusinessModel>{ HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var existing = await BusinessRepository.GetByOwnerUserIdAsync(ownerUserId, ct);
                if (existing != null)
                    return new BaseResponse<BusinessModel>
                        { HttpStatusCode = 400, Message = "You have already registered business" };
                
                if (string.IsNullOrWhiteSpace(business.Name))
                    return new BaseResponse<BusinessModel> {HttpStatusCode = 400, Message = "Name is required"};
                
                BusinessEntity b = new BusinessEntity
                {
                    Name = business.Name,
                    Email = business.Email,
                    Phone = business.Phone,
                    TimeZoneIana = business.TimeZoneIana,
                    SettingsJson = business.SettingsJson,
                    OwnerUserId = ownerUserId
                };
                
                int id = await BusinessRepository.CreateAsync(b, ct);

                return new BaseResponse<BusinessModel>
                {
                    Data = new BusinessModel
                    {
                        BusinessId = id,
                        Name = business.Name,
                        Email = business.Email,
                        Phone = business.Phone,
                        TimeZoneIana = business.TimeZoneIana,
                        SettingsJson = business.SettingsJson,
                    },
                    HttpStatusCode = 200,
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<BusinessModel> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> UpdateBusinessAsync(Guid ownerUserId, int businessId, UpdateBusinessModel business, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool>{ HttpStatusCode = 400, Message = "Invalid user Id"  };
                
                var existing = await BusinessRepository.GetByIdAsync(businessId, ct);
                if(existing == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };

                if (existing.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                existing.Name = business.Name ?? existing.Name;
                existing.Email = business.Email ?? existing.Email;
                existing.Phone = business.Phone ?? existing.Phone;
                existing.TimeZoneIana = business.TimeZoneIana?? existing.TimeZoneIana;
                existing.SettingsJson = business.SettingsJson ?? existing.SettingsJson;
                
                await BusinessRepository.UpdateAsync(existing, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteBusinessAsync(Guid ownerUserId, int businessId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var existing = await BusinessRepository.GetByIdAsync(businessId, ct);
                if(existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };
                
                if (existing.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> {HttpStatusCode = 403, Message = "Forbidden" };
                
                await BusinessRepository.DeleteAsync(businessId, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<BusinessModel>> GetMyBusinessAsync(Guid ownerUserId, int businessId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<BusinessModel>{HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<BusinessModel> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<BusinessModel> {HttpStatusCode = 403, Message = "Forbidden"};
                
                return new BaseResponse<BusinessModel>
                {
                    Data = new BusinessModel
                    {
                        BusinessId = business.BusinessId,
                        Name = business.Name,
                        Email = business.Email,
                        Phone = business.Phone,
                        TimeZoneIana = business.TimeZoneIana,
                        SettingsJson = business.SettingsJson,
                    },
                    HttpStatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BusinessModel>  { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<IEnumerable<CustomerModel>>> GetBusinessCustomers(Guid ownerUserId, int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<CustomerModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<CustomerModel>> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<IEnumerable<CustomerModel>> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var customers = await CustomerRepository.GetByBusinessIdAsync(businessId, pageNumber, pageSize, ct);
                List<CustomerModel> customerList = new List<CustomerModel>();

                foreach (var c in customers)
                {
                    customerList.Add(new CustomerModel
                    {
                        CustomerId = c.CustomerId,
                        BusinessId = c.BusinessId,
                        FullName = c.FullName,
                        Email = c.Email,
                        Phone = c.Phone,
                    });
                }
                
                return new BaseResponse<IEnumerable<CustomerModel>> { Data = customerList, HttpStatusCode = 200, Message = "Success" };
                
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<CustomerModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<ServiceModel>> AddServiceAsync(Guid ownerUserId, int businessId, CreateServiceModel service, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<ServiceModel>{ HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (string.IsNullOrWhiteSpace(service.Name))
                    return new BaseResponse<ServiceModel> { HttpStatusCode = 400, Message = "Name is required" };

                if (service.DurationMin < 5)
                    return new BaseResponse<ServiceModel> {HttpStatusCode = 400, Message = "Service duration must be at least 5 minutes"};
                
                if (service.Price < 0)
                    return new BaseResponse<ServiceModel> { HttpStatusCode = 400, Message = "Price must be positive"};
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<ServiceModel> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<ServiceModel> { HttpStatusCode = 403, Message = "Forbidden" };
                
                ServiceEntity s = new ServiceEntity
                {
                    BusinessId = businessId,
                    Name = service.Name.Trim(),
                    DurationMin = service.DurationMin,
                    Price = service.Price,
                    IsActive = service.IsActive,
                };

                int id = await ServiceRepository.CreateAsync(s, ct);

                return new BaseResponse<ServiceModel>
                {
                    Data = new ServiceModel
                    {
                        ServiceId = id,
                        BusinessId = businessId,
                        Name = service.Name,
                        DurationMin = service.DurationMin,
                        Price = service.Price,
                        IsActive = service.IsActive,
                    },
                    HttpStatusCode = 200
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<ServiceModel> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> UpdateServiceAsync(Guid ownerUserId, int businessId, int serviceId, UpdateServiceModel service, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (service.DurationMin < 5)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Duration must be at least 5 minutes" };
                
                if (service.Price < 0)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Price cannot be negative" };
                
                var existing = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Service with {serviceId} id not found" };
                
                if (existing.BusinessId != businessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} id not found"};
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                if (!string.IsNullOrWhiteSpace(service.Name))
                    existing.Name = service.Name.Trim();
                
                if (service.DurationMin.HasValue)
                    existing.DurationMin = service.DurationMin.Value;

                if (service.Price.HasValue)
                    existing.Price = service.Price.Value;
                
                await ServiceRepository.UpdateAsync(existing, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var existing = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Service with {serviceId} id not found" };
                
                if (existing.BusinessId != businessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} id not found"};
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                await ServiceRepository.DeleteAsync(serviceId, ct);
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> ActivateServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var existing = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Service with {serviceId} id not found" };
                
                if (existing.IsActive)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = $"Service is already activated" };
                
                if (existing.BusinessId != businessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                await ServiceRepository.ActivateAsync(serviceId, ct);
                
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    HttpStatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeactivateServiceAsync(Guid ownerUserId, int businessId, int serviceId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var existing = await ServiceRepository.GetByIdAsync(serviceId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Service with {serviceId} id not found" };
                
                if (existing.BusinessId != businessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                if (!existing.IsActive)
                    return new BaseResponse<bool> {HttpStatusCode = 400, Message = $"Service is already deactivated" };
                
                await ServiceRepository.DeactivateAsync(serviceId, ct);
                
                return new BaseResponse<bool> { Data = true, HttpStatusCode = 200 , Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<IEnumerable<ServiceModel>>> GetMyServicesAsync(Guid ownerUserId, int businessId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<ServiceModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<ServiceModel>> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<IEnumerable<ServiceModel>> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                var services = await ServiceRepository.GetByBusinessIdAsync(businessId, ct);
                
                List<ServiceModel> servicesList = new List<ServiceModel>();

                foreach (var s in services)
                {
                    servicesList.Add(new ServiceModel
                    {
                        ServiceId = s.ServiceId,
                        BusinessId = businessId,
                        Name = s.Name,
                        DurationMin = s.DurationMin,
                        Price = s.Price,
                        IsActive = s.IsActive,
                    });
                }

                return new BaseResponse<IEnumerable<ServiceModel>>
                {
                    Data = servicesList,
                    HttpStatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<ServiceModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<HoursModel>> AddOpeningHoursAsync(Guid ownerUserId, int businessId, CreateHoursModel hours, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<HoursModel> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (hours.DayOfWeek < 1 || hours.DayOfWeek > 7) 
                    return new  BaseResponse<HoursModel> { HttpStatusCode = 400, Message = "DayOfWeek must be between 1 and 7" };

                if (hours.CloseTime - hours.OpenTime < TimeSpan.FromMinutes(60))
                    return new  BaseResponse<HoursModel> { HttpStatusCode = 400, Message = "Business must be open at least 1 hour" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new  BaseResponse<HoursModel> { HttpStatusCode = 404, Message = $"Business with business {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<HoursModel> { HttpStatusCode = 403, Message = "Forbidden"  };
                
                OpeningHoursEntity o = new OpeningHoursEntity
                {
                    BusinessId = businessId,
                    DayOfWeek = hours.DayOfWeek,
                    OpenTime = hours.OpenTime,
                    CloseTime = hours.CloseTime,
                };
                
                var existing = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, hours.DayOfWeek, ct);
                if (existing != null)
                    return new BaseResponse<HoursModel>
                    {
                        HttpStatusCode = 400,
                        Message = "Opening hours for this day already exist for this business"
                    };

                int id = await OpeningHoursRepository.CreateAsync(o, ct);

                return new BaseResponse<HoursModel>
                {
                    Data = new HoursModel
                    {
                        OpeningHoursId = id,
                        BusinessId = businessId,
                        DayOfWeek = hours.DayOfWeek,
                        OpenTime = hours.OpenTime,
                        CloseTime = hours.CloseTime,
                    },
                    HttpStatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new  BaseResponse<HoursModel> {HttpStatusCode = 500, Message = ex.Message};
            }
        }

        public virtual async Task<BaseResponse<bool>> UpdateOpeningHoursAsync(Guid ownerUserId, int businessId, byte dow, UpdateHoursModel hours, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (dow < 1 || dow > 7) 
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "DayOfWeek must be between 1 and 7" };
                
                if (hours.CloseTime - hours.OpenTime < TimeSpan.FromMinutes(60))
                    return new  BaseResponse<bool> { HttpStatusCode = 400, Message = "Business must be open at least 1 hour" };

                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} id not found"};
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var existing = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow, ct);
                if (existing == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404,
                        Message = $"there is no opening hours for this {dow} day of week"};
                
                existing.OpenTime = hours.OpenTime;
                existing.CloseTime = hours.CloseTime;
                
                await OpeningHoursRepository.UpdateAsync(existing, ct);
                return new BaseResponse<bool> {Data = true, HttpStatusCode = 200, Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteOpeningHoursAsync(Guid ownerUserId, int businessId, byte dow, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (dow < 1 || dow > 7) 
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "DayOfWeek must be between 1 and 7" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404, Message = $"Business with {businessId} id not found"};
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var existing = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow, ct);
                if (existing == null)
                    return new BaseResponse<bool> {HttpStatusCode = 404,
                        Message = $"there is no opening hours for this {dow} day of week"};
                
                await OpeningHoursRepository.DeleteAsync(existing.OpeningHoursId, ct);
                return new BaseResponse<bool> {Data = true, HttpStatusCode = 200, Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<IEnumerable<HoursModel>>> GetMyOpeningHoursAsync(Guid ownerUserId, int businessId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<HoursModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<HoursModel>> { HttpStatusCode = 404, Message = $"Business with {businessId} not found" };
                
                if (ownerUserId != business.OwnerUserId)
                    return new BaseResponse<IEnumerable<HoursModel>> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var hours = await OpeningHoursRepository.GetByBusinessIdAsync(businessId, ct);
                List<HoursModel> hoursList = new List<HoursModel>();
                
                foreach (var h in hours)
                {
                    hoursList.Add(new HoursModel
                    {
                        OpeningHoursId = h.OpeningHoursId,
                        BusinessId = h.BusinessId,
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.OpenTime,
                        CloseTime = h.CloseTime,
                    });
                }

                return new BaseResponse<IEnumerable<HoursModel>>
                {
                    Data = hoursList,
                    HttpStatusCode = 200,
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<HoursModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<HolidayModel>> AddHolidayAsync(Guid ownerUserId, int businessId, CreateHolidayModel holiday, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<HolidayModel> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (holiday.HolidayDate.Date <= DateTime.UtcNow.Date)
                    return new BaseResponse<HolidayModel> { HttpStatusCode = 400, Message = "Holiday date cannot be in the past" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<HolidayModel>{ HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<HolidayModel> { HttpStatusCode = 403, Message = "Forbidden" };
                
                HolidayEntity h = new HolidayEntity
                {
                    BusinessId = business.BusinessId,
                    HolidayDate = holiday.HolidayDate,
                    Reason = holiday.Reason,
                };
            
                int id = await HolidayRepository.CreateAsync(h, ct);

                return new BaseResponse<HolidayModel>
                {
                    Data = new HolidayModel
                    {
                        HolidayId = id,
                        BusinessId = business.BusinessId,
                        HolidayDate = holiday.HolidayDate.Date,
                        Reason = holiday.Reason,
                    },
                    HttpStatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<HolidayModel> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> DeleteHolidayAsync(Guid ownerUserId, int businessId, int holidayId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool>{ HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var existing = await HolidayRepository.GetByIdAsync(holidayId, ct);
                if (existing == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Holiday with holiday {holidayId} id not found" };
                
                if (existing.BusinessId != businessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                await HolidayRepository.DeleteAsync(holidayId, ct);
                
                return new BaseResponse<bool> {Data = true, HttpStatusCode = 200, Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }
        
        public virtual async Task<BaseResponse<IEnumerable<BookingModel>>> GetCurrentActiveBookings(Guid ownerUserId, int businessId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 404, Message = $"Business with business {businessId} id not found" };

                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 403, Message = "Forbidden" };
                
                BookingFilter filter = new BookingFilter
                {
                    BusinessId = business.BusinessId,
                    Status = BookingStatus.Confirmed,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var bookings = await BookingRepository.GetAllSpecAsync(filter, ct);
                
                List<BookingModel> bookingsList = new List<BookingModel>();

                foreach (var b in bookings)
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
                        Notes = b.Notes,
                    });
                }

                return new BaseResponse<IEnumerable<BookingModel>>
                {
                    Data = bookingsList,
                    HttpStatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }
        
        public virtual async Task<BaseResponse<IEnumerable<BookingModel>>> GetAllBookingsAsync(Guid ownerUserId, int businessId, BookingRequest request, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 404, Message = $"Business with business {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 403, Message = "Forbidden" };
                
                BookingFilter filter = new BookingFilter
                {
                    BusinessId = business.BusinessId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Status = request.Status
                };
                
                var bookings = await BookingRepository.GetAllSpecAsync(filter, ct);
                
                List<BookingModel> bookingsList = new List<BookingModel>();

                foreach (var b in bookings)
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
                        Notes = b.Notes,
                    });
                }

                return new BaseResponse<IEnumerable<BookingModel>>
                {
                    Data = bookingsList,
                    HttpStatusCode = 200,
                };
                
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<IEnumerable<BookingModel>>> GetDailyBookingsAsync(Guid ownerUserId, int businessId, DateTime date, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                if (pageNumber < 1)
                    pageNumber = 1;
            
                if (pageSize < 1)
                    pageSize = 10;
            
                if(pageSize > 100)
                    pageSize = 100;
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 404, Message = $"Business with business {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 403, Message = "Forbidden" };
                
                // monday=1..sunday=7
                byte dow = (byte)(((int)date.DayOfWeek + 6) % 7 + 1);

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow, ct);
                if (hours == null)
                    return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 400, Message = $"Business doesn't have opening hours for the {dow} day of week"};

                DateTime day = date.Date;

                DateTime fromUtc = day.Add(hours.OpenTime);
                DateTime toUtc;

                if (hours.CloseTime == hours.OpenTime)
                {
                    // 24-hour open
                    toUtc = fromUtc.AddDays(1);
                }
                else if (hours.CloseTime > hours.OpenTime)
                {
                    // Normal same-day hours
                    toUtc = date.Date.Add(hours.CloseTime);
                }
                else
                {
                    // Overnight hours
                    toUtc = date.Date.Add(hours.CloseTime).AddDays(1);
                }
                
                var bookings = await BookingRepository.GetBookingsByRangeAsync(businessId, fromUtc, toUtc, pageNumber, pageSize, ct);
                
                List<BookingModel> bookingsList = new List<BookingModel>();

                foreach (var b in bookings)
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
                        Notes = b.Notes,
                    });
                }
                
                return new BaseResponse<IEnumerable<BookingModel>>
                {
                    Data = bookingsList,
                    HttpStatusCode = 200,
                };
                
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<BookingModel>> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> ConfirmBookingAsync(Guid ownerUserId, int businessId, int bookingId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var booking = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (booking == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Booking with {bookingId} id not found" };

                if (booking.BusinessId != business.BusinessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                if (booking.Status == "Confirmed")
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Booking is already confirmed" };
                
                if (booking.Status == "Cancelled")
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Booking is already cancelled" };
                
                BookingFilter filter = new BookingFilter
                {
                    BusinessId = businessId,
                    Date = booking.StartAtUtc,
                    Status = BookingStatus.Pending,
                };
                var otherBookings = await BookingRepository.GetAllSpecAsync(filter, ct);

                foreach (var b in otherBookings)
                {
                    if (b.BookingId != bookingId)
                    {
                        await BookingRepository.ChangeBookingStatusAsync(b.BookingId, "Cancelled", ct);
                    }
                }
                
                await BookingRepository.ChangeBookingStatusAsync(bookingId, "Confirmed", ct);
                return new BaseResponse<bool> {Data = true, HttpStatusCode = 200, Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }

        public virtual async Task<BaseResponse<bool>> CancelBookingAsync(Guid ownerUserId, int businessId, int bookingId, CancellationToken ct = default)
        {
            try
            {
                if (ownerUserId == Guid.Empty)
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Invalid user Id" };
                
                var business = await BusinessRepository.GetByIdAsync(businessId, ct);
                if (business == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Business with {businessId} id not found" };
                
                if (business.OwnerUserId != ownerUserId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                var booking = await BookingRepository.GetByIdAsync(bookingId, ct);
                if (booking == null)
                    return new BaseResponse<bool> { HttpStatusCode = 404, Message = $"Booking with {bookingId} id not found" };

                if (booking.BusinessId != business.BusinessId)
                    return new BaseResponse<bool> { HttpStatusCode = 403, Message = "Forbidden" };
                
                if (booking.Status == "Confirmed")
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Booking is already confirmed" };
                
                if (booking.Status == "Cancelled")
                    return new BaseResponse<bool> { HttpStatusCode = 400, Message = "Booking is already cancelled" };
                
                await BookingRepository.CancelAsync(bookingId, ct);

                return new BaseResponse<bool> {Data = true, HttpStatusCode = 200, Message = "Success"};
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { HttpStatusCode = 500, Message = ex.Message };
            }
        }
    }
}

