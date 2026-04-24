using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{

    public class AppointmentService
    {
        private readonly string _connectionString;

        public AppointmentService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task UpdateAppointmentAsync(AppointmentDisplay appt)
        {
            var error = await ValidateAppointmentAsync(appt.pet_id, appt.appointment_time, appt.Details.Select(d => d.service_id).ToList(), appt.Details.Select(d => (int?)d.employee_id).ToList(), appt.appointment_id);
            if (error != null) throw new Exception(error);

            using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            using var trans = db.BeginTransaction();
            try
            {
                await db.ExecuteAsync("UPDATE APPOINTMENT SET appointment_time = @appointment_time, pet_id = @pet_id, appointment_status = @appointment_status, used_reward_points = @used_reward_points WHERE appointment_id = @appointment_id", appt, trans);

                await db.ExecuteAsync("DELETE FROM COMMISSION_HISTORY WHERE appointment_detail_id IN (SELECT appointment_detail_id FROM APPOINTMENT_DETAIL WHERE appointment_id = @appointment_id)", appt, trans);
                await db.ExecuteAsync("DELETE FROM APPOINTMENT_DETAIL WHERE appointment_id = @appointment_id", appt, trans);
                foreach (var d in appt.Details)
                {
                    await db.ExecuteAsync("INSERT INTO APPOINTMENT_DETAIL (appointment_id, service_id, employee_id, price_at_booking) VALUES (@appointment_id, @service_id, @employee_id, @price_at_booking)", 
                        new { appointment_id = appt.appointment_id, d.service_id, d.employee_id, d.price_at_booking }, trans);
                }
                trans.Commit();
            }
            catch { trans.Rollback(); throw; }
        }

        public async Task<List<AppointmentDisplay>> GetAppointmentsForDisplayAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT 
                a.appointment_id, 
                a.pet_id,
                p.customer_id,
                a.appointment_time, 
                a.booking_time, 
                a.appointment_status, 
                a.payment_status, 
                a.used_reward_points,
                (SELECT ISNULL(SUM(d.price_at_booking), 0) FROM APPOINTMENT_DETAIL d WHERE d.appointment_id = a.appointment_id) AS total_amount,
                ISNULL(c.first_name + ' ' + c.last_name, 'N/A') AS customer_name,
                ISNULL(p.pet_name + ' (' + p.species + ')', 'N/A') AS pet_info,
                (SELECT TOP 1 ISNULL(e.first_name + ' ' + e.last_name, 'N/A') 
                 FROM APPOINTMENT_DETAIL ad 
                 LEFT JOIN EMPLOYEE e ON ad.employee_id = e.employee_id 
                 WHERE ad.appointment_id = a.appointment_id) AS employee_name
            FROM APPOINTMENT a
            LEFT JOIN PET p ON a.pet_id = p.pet_id
            LEFT JOIN CUSTOMER c ON p.customer_id = c.customer_id
            ORDER BY a.appointment_time DESC";

                var result = await db.QueryAsync<AppointmentDisplay>(sql);
                return result.ToList();
            }
        }
        public async Task<AppointmentDisplay> GetAppointmentByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string sqlMaster = @"
            SELECT 
                a.appointment_id, a.pet_id, p.customer_id, a.appointment_time, a.booking_time, 
                a.appointment_status, a.payment_status, a.used_reward_points,
                (SELECT ISNULL(SUM(d.price_at_booking), 0) FROM APPOINTMENT_DETAIL d WHERE d.appointment_id = a.appointment_id) AS total_amount, 
                ISNULL(c.first_name + ' ' + c.last_name, 'N/A') AS customer_name,
                ISNULL(c.address, 'N/A') AS customer_address,
                ISNULL(p.species + ' - ' + p.breed, 'N/A') AS pet_info,
                ISNULL(p.weight, 0) AS pet_weight
            FROM APPOINTMENT a
            LEFT JOIN PET p ON a.pet_id = p.pet_id
            LEFT JOIN CUSTOMER c ON p.customer_id = c.customer_id
            WHERE a.appointment_id = @Id";

                var appointment = await db.QueryFirstOrDefaultAsync<AppointmentDisplay>(sqlMaster, new { Id = id });


                if (appointment != null)
                {
                    string sqlDetails = @"
                SELECT 
                    d.appointment_detail_id,
                    d.service_id,
                    ISNULL(s.service_name, 'N/A') as service_name,
                    d.employee_id,
                    ISNULL(e.first_name + ' ' + e.last_name, 'N/A') AS detail_employee_name,
                    d.price_at_booking
                FROM APPOINTMENT_DETAIL d
                LEFT JOIN SERVICE s ON d.service_id = s.service_id
                LEFT JOIN EMPLOYEE e ON d.employee_id = e.employee_id
                WHERE d.appointment_id = @Id";

                    var details = await db.QueryAsync<AppointmentDetailItem>(sqlDetails, new { Id = id });
                    appointment.Details = details.ToList();
                }

                return appointment;
            }
        }

        public async Task<string?> ValidateAppointmentAsync(int petId, DateTime time, List<int> serviceIds, List<int?> employeeIds, int? excludeApptId = null)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();




                string durSql = "SELECT ISNULL(SUM(estimated_duration), 0) FROM SERVICE WHERE service_id IN @serviceIds";
                int newDuration = await db.ExecuteScalarAsync<int>(durSql, new { serviceIds });
                if (newDuration == 0) newDuration = 30;
                
                DateTime newEnd = time.AddMinutes(newDuration);


                string checkSql = @"
                    SELECT 
                        a.appointment_id, 
                        a.appointment_time as StartTime,
                        (SELECT ISNULL(NULLIF(SUM(s.estimated_duration), 0), 30) 
                         FROM APPOINTMENT_DETAIL ad 
                         JOIN SERVICE s ON ad.service_id = s.service_id 
                         WHERE ad.appointment_id = a.appointment_id) as Duration,
                        a.pet_id,
                        ad_emp.employee_id
                    FROM APPOINTMENT a
                    LEFT JOIN APPOINTMENT_DETAIL ad_emp ON a.appointment_id = ad_emp.appointment_id
                    WHERE a.appointment_status != 'Cancelled'
                    AND CAST(a.appointment_time AS DATE) = CAST(@time AS DATE)
                    " + (excludeApptId.HasValue ? " AND a.appointment_id != @excludeApptId" : "");

                var existing = await db.QueryAsync<dynamic>(checkSql, new { time, excludeApptId });

                foreach (var ex in existing)
                {
                    DateTime exStart = (DateTime)ex.StartTime;
                    DateTime exEnd = exStart.AddMinutes((int)ex.Duration);


                    bool overlaps = (time < exEnd) && (newEnd > exStart);

                    if (overlaps)
                    {
                        if (ex.pet_id == petId)
                            return $"Thú cưng đã có lịch hẹn #{ex.appointment_id} từ {exStart:HH:mm} đến {exEnd:HH:mm}.";

                        if (ex.employee_id != null && employeeIds.Contains((int)ex.employee_id))
                        {
                            var name = await db.ExecuteScalarAsync<string>("SELECT first_name + ' ' + last_name FROM EMPLOYEE WHERE employee_id = @id", new { id = ex.employee_id });
                            return $"Nhân viên {name} đã bận lịch hẹn #{ex.appointment_id} từ {exStart:HH:mm} đến {exEnd:HH:mm}.";
                        }
                    }
                }
                return null;
            }
        }

        public async Task<int> AddAppointmentAsync(AppointmentCreateRequest req)
        {
            var error = await ValidateAppointmentAsync(req.pet_id, req.appointment_time, req.Details.Select(d => d.service_id).ToList(), req.Details.Select(d => (int?)d.employee_id).ToList());
            if (error != null) throw new Exception(error);

            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var trans = db.BeginTransaction())
                {
                    try
                    {
                        int usedPoints = req.used_reward_points ?? 0;
                        decimal totalAmount = req.Details.Sum(d => d.price_at_booking);
                        decimal finalAmount = totalAmount - usedPoints;
                        if (finalAmount < 0) finalAmount = 0;
                        int pointsEarned = (int)(finalAmount * 0.01m);

                        string sqlAppt = @"
                            INSERT INTO APPOINTMENT (pet_id, appointment_time, booking_time, appointment_status, payment_status, used_reward_points)
                            VALUES (@pet_id, @appointment_time, SYSDATETIMEOFFSET(), 'Confirmed', @payment_status, @used_reward_points);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        
                        int apptId = await db.QuerySingleAsync<int>(sqlAppt, new { req.pet_id, req.appointment_time, req.payment_status, used_reward_points = usedPoints }, trans);

                        string sqlDetail = @"
                            INSERT INTO APPOINTMENT_DETAIL (appointment_id, service_id, employee_id, price_at_booking)
                            VALUES (@appointment_id, @service_id, NULLIF(@employee_id, 0), @price_at_booking)";

                        foreach (var d in req.Details)
                        {
                            await db.ExecuteAsync(sqlDetail, new { appointment_id = apptId, d.service_id, d.employee_id, d.price_at_booking }, trans);
                        }

                        if (req.payment_status == "Completed")
                        {
                            var details = await db.QueryAsync(@"
                                SELECT ad.appointment_detail_id, ad.price_at_booking, ad.employee_id 
                                FROM APPOINTMENT_DETAIL ad
                                JOIN EMPLOYEE e ON ad.employee_id = e.employee_id
                                WHERE ad.appointment_id = @id AND e.role_id != 1", new { id = apptId }, trans);
                            
                            string rateStr = await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = 'service_comm'", null, trans);
                            decimal commRate = decimal.TryParse(rateStr, out var r) ? r : 10.0m;

                            foreach (var d in details)
                            {
                                if (d.employee_id > 0)
                                {
                                    decimal commAmount = (decimal)d.price_at_booking * (commRate / 100);
                                    await db.ExecuteAsync(@"INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, recorded_time, appointment_detail_id)
                                                            VALUES (N'Dịch vụ', @rate, @amount, SYSDATETIMEOFFSET(), @did)", 
                                                            new { rate = commRate, amount = commAmount, did = d.appointment_detail_id }, trans);
                                }
                            }
                        }

                        trans.Commit();
                        return apptId;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            await db.ExecuteAsync("UPDATE APPOINTMENT SET appointment_status = @status WHERE appointment_id = @id", new { id, status });
        }

        public async Task UpdateAppointmentPaymentAsync(int id, string status)
        {
            using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            using var trans = db.BeginTransaction();
            try
            {
                await db.ExecuteAsync("UPDATE APPOINTMENT SET payment_status = @status WHERE appointment_id = @id", new { id, status }, trans);
                
                if (status == "Completed")
                {

                    var details = await db.QueryAsync(@"
                        SELECT ad.appointment_detail_id, ad.price_at_booking, ad.employee_id 
                        FROM APPOINTMENT_DETAIL ad
                        JOIN EMPLOYEE e ON ad.employee_id = e.employee_id
                        WHERE ad.appointment_id = @id AND e.role_id != 1", new { id }, trans);
                    string rateStr = await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = 'service_comm'", null, trans);
                    decimal commRate = decimal.TryParse(rateStr, out var r) ? r : 10.0m;

                    foreach (var d in details)
                    {
                        bool exists = await db.ExecuteScalarAsync<bool>("SELECT 1 FROM COMMISSION_HISTORY WHERE appointment_detail_id = @did", new { did = d.appointment_detail_id }, trans);
                        if (!exists && d.employee_id > 0)
                        {
                            decimal commAmount = (decimal)d.price_at_booking * (commRate / 100);
                            await db.ExecuteAsync(@"INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, recorded_time, appointment_detail_id)
                                                    VALUES (N'Dịch vụ', @rate, @amount, SYSDATETIMEOFFSET(), @did)", 
                                                    new { rate = commRate, amount = commAmount, did = d.appointment_detail_id }, trans);
                        }
                    }
                }
                else
                {

                    await db.ExecuteAsync(@"DELETE FROM COMMISSION_HISTORY 
                                           WHERE appointment_detail_id IN (SELECT appointment_detail_id FROM APPOINTMENT_DETAIL WHERE appointment_id = @id)", 
                                           new { id }, trans);
                }
                trans.Commit();
            }
            catch { trans.Rollback(); throw; }
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();
            using var trans = db.BeginTransaction();
            try
            {

                await db.ExecuteAsync(@"DELETE FROM COMMISSION_HISTORY 
                                        WHERE appointment_detail_id IN (SELECT appointment_detail_id FROM APPOINTMENT_DETAIL WHERE appointment_id = @id)", 
                                        new { id }, trans);
                
                await db.ExecuteAsync("DELETE FROM APPOINTMENT_DETAIL WHERE appointment_id = @id", new { id }, trans);
                await db.ExecuteAsync("DELETE FROM APPOINTMENT WHERE appointment_id = @id", new { id }, trans);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
        public async Task<List<AppointmentDisplay>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        a.*, 
                        p.pet_name + ' (' + p.species + ')' AS pet_info,
                        c.first_name + ' ' + c.last_name AS customer_name,
                        pn.num AS customer_phone,
                        (SELECT STRING_AGG(s.service_name, ', ') 
                         FROM APPOINTMENT_DETAIL ad 
                         JOIN SERVICE s ON ad.service_id = s.service_id 
                         WHERE ad.appointment_id = a.appointment_id) AS service_names
                    FROM APPOINTMENT a
                    JOIN PET p ON a.pet_id = p.pet_id
                    JOIN CUSTOMER c ON p.customer_id = c.customer_id
                    LEFT JOIN PHONE_NUM pn ON c.customer_id = pn.customer_id
                    WHERE p.customer_id = @customerId
                    ORDER BY a.appointment_time DESC";

                var result = await db.QueryAsync<AppointmentDisplay>(sql, new { customerId });
                return result.ToList();
            }
        }
    }
}

