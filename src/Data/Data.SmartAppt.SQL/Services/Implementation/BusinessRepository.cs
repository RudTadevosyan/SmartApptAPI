using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class BusinessRepository : IBusinessRepository
    {
        protected readonly IDbConnection Connection;

        public BusinessRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(BusinessEntity businessData, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(businessData.Name) ? businessData.Name : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(businessData.Email) ? businessData.Email : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(businessData.Phone) ? businessData.Phone : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@TimeZoneIana", SqlDbType.NVarChar, 100)
            { Value = !string.IsNullOrEmpty(businessData.TimeZoneIana) ? businessData.TimeZoneIana : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1)
            { Value = !string.IsNullOrEmpty(businessData.SettingsJson) ? businessData.SettingsJson : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@OwnerUserId", SqlDbType.UniqueIdentifier)
            { Value = businessData.OwnerUserId });

            var output = new SqlParameter("@BusinessId", SqlDbType.Int)
            { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task<BusinessEntity?> GetByIdAsync(int businessId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {
                return new BusinessEntity
                {
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    TimeZoneIana = reader.GetString(reader.GetOrdinal("TimeZoneIana")),
                    SettingsJson = reader.IsDBNull(reader.GetOrdinal("SettingsJson")) ? null : reader.GetString(reader.GetOrdinal("SettingsJson")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
                    OwnerUserId = reader.GetGuid(reader.GetOrdinal("OwnerUserId"))
                };
            }

            return null;
        }
        
        public virtual async Task<BusinessEntity?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_GetByOwnerUserId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OwnerUserId", SqlDbType.UniqueIdentifier) { Value = ownerUserId });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {
                return new BusinessEntity
                {
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    TimeZoneIana = reader.GetString(reader.GetOrdinal("TimeZoneIana")),
                    SettingsJson = reader.IsDBNull(reader.GetOrdinal("SettingsJson")) ? null : reader.GetString(reader.GetOrdinal("SettingsJson")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
                    OwnerUserId = reader.GetGuid(reader.GetOrdinal("OwnerUserId"))
                };
            }

            return null;
        }

        public virtual async Task<IEnumerable<BusinessEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            var businesses = new List<BusinessEntity>();

            int ordBusinessId = reader.GetOrdinal("BusinessId");
            int ordName = reader.GetOrdinal("Name");
            int ordEmail = reader.GetOrdinal("Email");
            int ordPhone = reader.GetOrdinal("Phone");
            int ordTimeZoneIana = reader.GetOrdinal("TimeZoneIana");
            int ordSettingsJson = reader.GetOrdinal("SettingsJson");
            int ordCreatedAtUtc = reader.GetOrdinal("CreatedAtUtc");

            while (await reader.ReadAsync(ct))
            {
                businesses.Add(new BusinessEntity
                {
                    BusinessId = reader.GetInt32(ordBusinessId),
                    Name = reader.GetString(ordName),
                    Email = reader.IsDBNull(ordEmail) ? null : reader.GetString(ordEmail),
                    Phone = reader.IsDBNull(ordPhone) ? null : reader.GetString(ordPhone),
                    TimeZoneIana = reader.GetString(ordTimeZoneIana),
                    SettingsJson = reader.IsDBNull(ordSettingsJson) ? null : reader.GetString(ordSettingsJson),
                    CreatedAtUtc = reader.GetDateTime(ordCreatedAtUtc)
                });
            }

            return businesses;
        }

        public virtual async Task UpdateAsync(BusinessEntity businessData, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessData.BusinessId });

            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(businessData.Name) ? businessData.Name : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(businessData.Email) ? businessData.Email : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(businessData.Phone) ? businessData.Phone : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@TimeZoneIana", SqlDbType.NVarChar, 100)
            { Value = !string.IsNullOrEmpty(businessData.TimeZoneIana) ? businessData.TimeZoneIana : DBNull.Value });

            cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1)
            { Value = !string.IsNullOrEmpty(businessData.SettingsJson) ? businessData.SettingsJson : DBNull.Value });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task DeleteAsync(int businessId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}