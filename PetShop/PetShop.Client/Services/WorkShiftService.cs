using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class WorkShiftService
    {
        private readonly string _conn;
        public WorkShiftService(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection");

        public async Task<List<WorkShiftDisplay>> GetAllWorkShiftsAsync()
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                SELECT 
                    w.workshift_id, w.employee_id, w.start_time, w.end_time,
                    ISNULL(e.first_name + ' ' + ISNULL(e.middle_name + ' ', '') + e.last_name, 'N/A') AS employee_name
                FROM WORK_SHIFT w
                LEFT JOIN EMPLOYEE e ON w.employee_id = e.employee_id
                ORDER BY w.start_time DESC";

            return (await db.QueryAsync<WorkShiftDisplay>(sql)).ToList();
        }

        public async Task AddWorkShiftAsync(WorkShiftDisplay ws)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"INSERT INTO WORK_SHIFT (employee_id, start_time, end_time) 
                           VALUES (@employee_id, @start_time, @end_time)";
            await db.ExecuteAsync(sql, ws);
        }

        public async Task UpdateWorkShiftAsync(WorkShiftDisplay ws)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"UPDATE WORK_SHIFT 
                           SET employee_id = @employee_id, start_time = @start_time, end_time = @end_time
                           WHERE workshift_id = @workshift_id";
            await db.ExecuteAsync(sql, ws);
        }

        public async Task<bool> IsShiftActiveAsync(int employeeId)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"SELECT COUNT(1) FROM WORK_SHIFT 
                           WHERE employee_id = @employeeId 
                             AND CAST(start_time AS DATE) = CAST(GETDATE() AS DATE)
                             AND end_time > GETDATE()";
            return await db.ExecuteScalarAsync<int>(sql, new { employeeId }) > 0;
        }

        public async Task<int> GetOrCreateCurrentShiftAsync(int employeeId)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string cleanupSql = @"
                DELETE FROM WORK_SHIFT 
                WHERE employee_id = @employeeId 
                  AND end_time > start_time -- placeholder for open shift
                  AND end_time > DATEADD(hour, 1, start_time) -- and not yet closed (closed shifts usually take a few hours)
                  AND CAST(start_time AS DATE) < CAST(GETDATE() AS DATE)";
            

            string deleteStaleSql = @"
                DELETE FROM WORK_SHIFT 
                WHERE employee_id = @employeeId 
                  AND end_time > GETDATE() 
                  AND CAST(start_time AS DATE) < CAST(GETDATE() AS DATE)";
            await db.ExecuteAsync(deleteStaleSql, new { employeeId });

            string checkSql = @"
                SELECT TOP 1 workshift_id 
                FROM WORK_SHIFT 
                WHERE employee_id = @employeeId 
                  AND CAST(start_time AS DATE) = CAST(GETDATE() AS DATE)
                  AND end_time > GETDATE()
                ORDER BY start_time DESC";
            
            int existingId = await db.QueryFirstOrDefaultAsync<int>(checkSql, new { employeeId });
            if (existingId > 0) return existingId;


            string insertSql = @"INSERT INTO WORK_SHIFT (employee_id, start_time, end_time) 
                                VALUES (@employeeId, GETDATE(), DATEADD(day, 1, GETDATE()));
                                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await db.ExecuteScalarAsync<int>(insertSql, new { employeeId });
        }

        public async Task CloseShiftAsync(int employeeId)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                UPDATE WORK_SHIFT 
                SET end_time = GETDATE() 
                WHERE employee_id = @employeeId 
                  AND CAST(start_time AS DATE) = CAST(GETDATE() AS DATE)
                  AND end_time > GETDATE()";
            await db.ExecuteAsync(sql, new { employeeId });
        }

        public async Task<int> StartShiftAsync(int employeeId) => await GetOrCreateCurrentShiftAsync(employeeId);
        public async Task EndShiftAsync(int employeeId) => await CloseShiftAsync(employeeId);

        public async Task DeleteWorkShiftAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = "DELETE FROM WORK_SHIFT WHERE workshift_id = @id";
            await db.ExecuteAsync(sql, new { id });
        }

        public async Task<string> GetQRSecretAsync()
        {
            using IDbConnection db = new SqlConnection(_conn);
            try {
                string sql = "SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = 'qr_secret'";
                var secret = await db.QueryFirstOrDefaultAsync<string>(sql);
                return secret ?? "PETSHOP_OFFICE_2026";
            } catch { return "PETSHOP_OFFICE_2026"; }
        }

        public async Task SetQRSecretAsync(string secret)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'STORE_SETTINGS')
                BEGIN
                    CREATE TABLE STORE_SETTINGS (
                        setting_key NVARCHAR(100) PRIMARY KEY,
                        setting_value NVARCHAR(MAX)
                    );
                END

                IF EXISTS (SELECT 1 FROM STORE_SETTINGS WHERE setting_key = 'qr_secret')
                    UPDATE STORE_SETTINGS SET setting_value = @secret WHERE setting_key = 'qr_secret'
                ELSE
                    INSERT INTO STORE_SETTINGS (setting_key, setting_value) VALUES ('qr_secret', @secret)";
            
            try { await db.ExecuteAsync(sql, new { secret }); } catch { }
        }
    }
}
