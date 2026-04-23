using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class OpeningHoursRepository : IOpeningHoursRepository
    {
        protected readonly IDbConnection Connection;

        public OpeningHoursRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(OpeningHoursEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = entity.DayOfWeek });
            cmd.Parameters.Add(new SqlParameter("@OpenTime", SqlDbType.Time) { Value = entity.OpenTime });
            cmd.Parameters.Add(new SqlParameter("@CloseTime", SqlDbType.Time) { Value = entity.CloseTime });

            var output = new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int hoursId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = hoursId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<IEnumerable<OpeningHoursEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            var list = new List<OpeningHoursEntity>();

            int ordOpeningHoursId = reader.GetOrdinal("OpeningHoursId");
            int ordBusinessId = reader.GetOrdinal("BusinessId");
            int ordDayOfWeek = reader.GetOrdinal("DayOfWeek");
            int ordOpenTime = reader.GetOrdinal("OpenTime");
            int ordCloseTime = reader.GetOrdinal("CloseTime");

            while (await reader.ReadAsync(ct))
            {
                list.Add(new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(ordOpeningHoursId),
                    BusinessId = reader.GetInt32(ordBusinessId),
                    DayOfWeek = reader.GetByte(ordDayOfWeek),
                    OpenTime = reader.GetTimeSpan(ordOpenTime),
                    CloseTime = reader.GetTimeSpan(ordCloseTime)
                });
            }

            return list;
        }

        public virtual async Task<IEnumerable<OpeningHoursEntity>> GetByBusinessIdAsync(int businessId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            var result = new List<OpeningHoursEntity>();
            using var cmd = new SqlCommand("core.OpeningHours_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                });
            }

            return result;
        }

        public virtual async Task<OpeningHoursEntity?> GetByBusinessIdAndDowAsync(int businessId, byte dayOfWeek, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_GetByBusinessIdAndDow", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = dayOfWeek });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                };
            }

            return null;
        }

        public virtual async Task<OpeningHoursEntity?> GetByIdAsync(int hoursId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = hoursId });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(OpeningHoursEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = entity.OpeningHoursId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = entity.DayOfWeek });
            cmd.Parameters.Add(new SqlParameter("@OpenTime", SqlDbType.Time) { Value = entity.OpenTime });
            cmd.Parameters.Add(new SqlParameter("@CloseTime", SqlDbType.Time) { Value = entity.CloseTime });

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}