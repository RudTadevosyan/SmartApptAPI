using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class HolidayRepository : IHolidayRepository
    {
        protected readonly IDbConnection Connection;

        public HolidayRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(HolidayEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = entity.HolidayDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.Reason) ? entity.Reason : DBNull.Value });

            var output = new SqlParameter("@HolidayId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int holidayId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = holidayId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<IEnumerable<HolidayEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            var holidays = new List<HolidayEntity>();

            int ordHolidayId = reader.GetOrdinal("HolidayId");
            int ordBusinessId = reader.GetOrdinal("BusinessId");
            int ordHolidayDate = reader.GetOrdinal("HolidayDate");
            int ordReason = reader.GetOrdinal("Reason");

            while (await reader.ReadAsync(ct))
            {
                holidays.Add(new HolidayEntity
                {
                    HolidayId = reader.GetInt32(ordHolidayId),
                    BusinessId = reader.GetInt32(ordBusinessId),
                    HolidayDate = reader.GetDateTime(ordHolidayDate),
                    Reason = reader.IsDBNull(ordReason) ? null : reader.GetString(ordReason)
                });
            }

            return holidays;
        }

        public virtual async Task<HolidayEntity?> GetByBusinessIdAsync(int businessId, DateTime date, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = date.Date });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = businessId,
                    HolidayDate = date,
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                };
            }

            return null;
        }

        public virtual async Task<HolidayEntity?> GetByIdAsync(int holidayId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = holidayId });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(HolidayEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = entity.HolidayId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = entity.HolidayDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.Reason) ? entity.Reason : DBNull.Value });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<List<HolidayEntity>> GetAllByMonthAsync(int businessId, int year, int month, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            var holidays = new List<HolidayEntity>();
            using var cmd = new SqlCommand("core.Holiday_GetAllByMonth", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@Year", SqlDbType.Int) { Value = year });
            cmd.Parameters.Add(new SqlParameter("@Month", SqlDbType.Int) { Value = month });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                holidays.Add(new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                });
            }

            return holidays;
        }
    }
}