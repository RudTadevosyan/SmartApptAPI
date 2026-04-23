using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class ServiceRepository : IServiceRepository
    {
        protected readonly IDbConnection Connection;

        public ServiceRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(ServiceEntity service, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = string.IsNullOrEmpty(service.Name) ? DBNull.Value : service.Name });
            cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });
            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });
            cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = service.Price
            });

            var output = new SqlParameter("@ServiceId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task<ServiceEntity?> GetByIdAsync(int serviceId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ServiceEntity
                {
                    ServiceId = serviceId,
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    DurationMin = reader.GetInt32(reader.GetOrdinal("DurationMin")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                };
            }

            return null;
        }

        public virtual async Task<IEnumerable<ServiceEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageSize });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageNumber });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            var services = new List<ServiceEntity>();

            int ordServiceId = reader.GetOrdinal("ServiceId");
            int ordBusinessId = reader.GetOrdinal("BusinessId");
            int ordName = reader.GetOrdinal("Name");
            int ordDurationMin = reader.GetOrdinal("DurationMin");
            int ordPrice = reader.GetOrdinal("Price");
            int ordIsActive = reader.GetOrdinal("IsActive");

            while (await reader.ReadAsync(ct))
            {
                services.Add(new ServiceEntity
                {
                    ServiceId = reader.GetInt32(ordServiceId),
                    BusinessId = reader.GetInt32(ordBusinessId),
                    Name = reader.GetString(ordName),
                    DurationMin = reader.GetInt32(ordDurationMin),
                    Price = reader.GetDecimal(ordPrice),
                    IsActive = reader.GetBoolean(ordIsActive),
                });
            }

            return services;
        }

        public virtual async Task<IEnumerable<ServiceEntity>> GetByBusinessIdAsync(int businessId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            var ordServiceId = reader.GetOrdinal("ServiceId");
            var ordBusinessId = reader.GetOrdinal("BusinessId");
            var ordName = reader.GetOrdinal("Name");
            var ordDurationMin = reader.GetOrdinal("DurationMin");
            var ordPrice = reader.GetOrdinal("Price");
            var ordIsActive = reader.GetOrdinal("IsActive");

            var services = new List<ServiceEntity>();

            while (await reader.ReadAsync(ct))
            {
                services.Add(new ServiceEntity
                {
                    ServiceId = reader.GetInt32(ordServiceId),
                    BusinessId = reader.GetInt32(ordBusinessId),
                    Name = reader.GetString(ordName),
                    DurationMin = reader.GetInt32(ordDurationMin),
                    Price = reader.GetDecimal(ordPrice),
                    IsActive = reader.GetBoolean(ordIsActive)
                });
            }

            return services;
        }

        public virtual async Task UpdateAsync(ServiceEntity service, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = service.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = string.IsNullOrEmpty(service.Name) ? DBNull.Value : service.Name });
            cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });
            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });
            cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = service.Price
            });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task DeleteAsync(int serviceId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task DeactivateAsync(int serviceId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Deactivate", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task ActivateAsync(int serviceId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Activate", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}