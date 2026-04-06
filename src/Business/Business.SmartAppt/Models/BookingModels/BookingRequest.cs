using Data.SmartAppt.SQL.Models;

namespace Business.SmartAppt.Models.BookingModels;

public class BookingRequest
{
    public BookingStatus? Status { get; set; }
}