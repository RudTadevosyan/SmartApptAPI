using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        protected readonly IDbConnection Connection;

        public BookingRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task CancelAsync(int bookingId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_Cancel", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task ChangeBookingStatusAsync(int bookingId, string status, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_ChangeStatus", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 12) { Value = status });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<IEnumerable<BookingEntity>> GetBookingsByRangeAsync(
            int businessId, DateTime from, DateTime to, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetBookingsByRange", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2) { Value = from });
            cmd.Parameters.Add(new SqlParameter("@EndAtUtc", SqlDbType.DateTime2) { Value = to });
            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            int ordBookingId = reader.GetOrdinal("BookingId");
            int ordServiceId = reader.GetOrdinal("ServiceId");
            int ordCustomerId = reader.GetOrdinal("CustomerId");
            int ordStart = reader.GetOrdinal("StartAtUtc");
            int ordEnd = reader.GetOrdinal("EndAtUtc");
            int ordStatus = reader.GetOrdinal("Status");
            int ordNotes = reader.GetOrdinal("Notes");

            List<BookingEntity> bookings = new List<BookingEntity>();

            while (await reader.ReadAsync(ct))
            {
                bookings.Add(new BookingEntity
                {
                    BookingId = reader.GetInt32(ordBookingId),
                    BusinessId = businessId,
                    ServiceId = reader.GetInt32(ordServiceId),
                    CustomerId = reader.GetInt32(ordCustomerId),
                    StartAtUtc = reader.GetDateTime(ordStart),
                    EndAtUtc = reader.GetDateTime(ordEnd),
                    Status = reader.GetString(ordStatus),
                    Notes = reader.IsDBNull(ordNotes) ? null : reader.GetString(ordNotes),
                });
            }

            return bookings;
        }

        public virtual async Task<int> CreateAsync(BookingEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_SafeCreate", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = entity.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2) { Value = entity.StartAtUtc });
            cmd.Parameters.Add(new SqlParameter("@EndAtUtc", SqlDbType.DateTime2) { Value = entity.EndAtUtc });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500)
            { Value = !string.IsNullOrEmpty(entity.Notes) ? entity.Notes : DBNull.Value });

            var output = new SqlParameter("@BookingId", SqlDbType.Int)
            { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync(ct);
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int bookingId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<IEnumerable<BookingEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            int ordBookingId = reader.GetOrdinal("BookingId");
            int ordBusinessId = reader.GetOrdinal("BusinessId");
            int ordServiceId = reader.GetOrdinal("ServiceId");
            int ordCustomerId = reader.GetOrdinal("CustomerId");
            int ordStart = reader.GetOrdinal("StartAtUtc");
            int ordEnd = reader.GetOrdinal("EndAtUtc");
            int ordStatus = reader.GetOrdinal("Status");
            int ordNotes = reader.GetOrdinal("Notes");

            List<BookingEntity> bookings = new List<BookingEntity>();

            while (await reader.ReadAsync(ct))
            {
                bookings.Add(new BookingEntity
                {
                    BookingId = reader.GetInt32(ordBookingId),
                    BusinessId = reader.GetInt32(ordBusinessId),
                    ServiceId = reader.GetInt32(ordServiceId),
                    CustomerId = reader.GetInt32(ordCustomerId),
                    StartAtUtc = reader.GetDateTime(ordStart),
                    EndAtUtc = reader.GetDateTime(ordEnd),
                    Status = reader.GetString(ordStatus),
                    Notes = reader.IsDBNull(ordNotes) ? null : reader.GetString(ordNotes),
                });
            }

            return bookings;
        }

        public virtual async Task<IEnumerable<BookingEntity>> GetAllSpecAsync(BookingFilter filter, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetAllSpec", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = filter.BusinessId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = filter.ServiceId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = filter.CustomerId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 12)
            {
                Value = filter.Status.HasValue ? filter.Status.Value.ToString() : DBNull.Value
            });
            cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.Date) { Value = filter.Date ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = filter.PageNumber ?? 1 });
            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = filter.PageSize ?? 10 });

            var bookings = new List<BookingEntity>();

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                bookings.Add(new BookingEntity
                {
                    BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                    EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                });
            }

            return bookings;
        }

        public virtual async Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            using var reader = await cmd.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {
                return new BookingEntity
                {
                    BookingId = bookingId,
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                    EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(BookingEntity entity, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_SafeUpdate", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = entity.BookingId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = entity.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2) { Value = entity.StartAtUtc });
            cmd.Parameters.Add(new SqlParameter("@EndAtUtc", SqlDbType.DateTime2) { Value = entity.EndAtUtc });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500)
            { Value = !string.IsNullOrEmpty(entity.Notes) ? entity.Notes : DBNull.Value });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public virtual async Task<Dictionary<DateOnly, int>> GetBookingsCountByBusinessAndRangeAsync(
            int businessId, int serviceId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetBookingsCountByBusinessAndRange", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) });
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) });

            var result = new Dictionary<DateOnly, int>();

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var date = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("BookingDate")));
                var count = reader.GetInt32(reader.GetOrdinal("BookingCount"));
                result[date] = count;
            }

            return result;
        }
    }
}