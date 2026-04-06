namespace Data.SmartAppt.SQL.Models
{
    public class BookingFilter
    {
        public int? BusinessId { get; set; }
        public int? CustomerId { get; set; }
        public int? ServiceId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? Date {  get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

    }
}
