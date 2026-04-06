namespace Business.SmartAppt.Models;

public class CustomerModel
{
    public int CustomerId { get; set; }
    public int BusinessId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}