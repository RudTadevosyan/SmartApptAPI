using Data.SmartAppt.SQL.Models;


namespace Data.SmartAppt.SQL.Services
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<CustomerEntity>> GetByBusinessIdAsync(int businessId, int pageNumber = 1, int pageSize = 10);
        Task<CustomerEntity?> GetByIdAsync(int customerId);
        Task<int> CreateAsync(CustomerEntity entity);
        Task UpdateAsync(CustomerEntity customerEntity);
        Task DeleteAsync(int customerId);
    }
}
