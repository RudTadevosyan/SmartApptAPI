namespace Data.SmartAppt.SQL.Models
{
    public class CustomerEntity
    {
        public int CustomerId { get; set; }
        public int BusinessId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

    }
}
