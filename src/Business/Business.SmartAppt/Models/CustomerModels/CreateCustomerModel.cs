namespace Business.SmartAppt.Models.CustomerModels;

public class CreateCustomerModel
{
    public int BusinessId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}