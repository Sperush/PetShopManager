using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class CommissionService
    {
        private readonly string _conn;
        public CommissionService(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection");

        public async Task<List<CommissionDisplay>> GetCommissionsAsync()
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                SELECT 
                    c.*,
                    ad.appointment_id,
                    od.order_id,
                    COALESCE(e_ad.first_name + ' ' + e_ad.last_name, e_o.first_name + ' ' + e_o.last_name, 'N/A') AS employee_name,
                    COALESCE(e_ad.employee_id, e_o.employee_id, 0) AS employee_id,
                    COALESCE(pn_ad.num, pn_o.num, '') AS employee_phone
                FROM COMMISSION_HISTORY c
                LEFT JOIN APPOINTMENT_DETAIL ad ON c.appointment_detail_id = ad.appointment_detail_id
                LEFT JOIN EMPLOYEE e_ad ON ad.employee_id = e_ad.employee_id
                LEFT JOIN PHONE_NUM pn_ad ON e_ad.employee_id = pn_ad.employee_id
                
                LEFT JOIN ORDER_DETAIL od ON c.order_detail_id = od.order_detail_id
                LEFT JOIN ORDERS o ON od.order_id = o.order_id
                LEFT JOIN EMPLOYEE e_o ON o.employee_id = e_o.employee_id
                LEFT JOIN PHONE_NUM pn_o ON e_o.employee_id = pn_o.employee_id
                ORDER BY c.recorded_time DESC";

            return (await db.QueryAsync<CommissionDisplay>(sql)).ToList();
        }

        public async Task<decimal> GetGlobalRateAsync(string key)
        {
            using IDbConnection db = new SqlConnection(_conn);
            try {
                var val = await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = @key", new { key });
                return string.IsNullOrEmpty(val) ? 5.0m : decimal.Parse(val);
            } catch { return 5.0m; }
        }

        public async Task<string> GetSettingAsync(string key, string defaultVal = "")
        {
            using IDbConnection db = new SqlConnection(_conn);
            try {
                return await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = @key", new { key }) ?? defaultVal;
            } catch { return defaultVal; }
        }

        public async Task SetSettingAsync(string key, string value)
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
                IF EXISTS (SELECT 1 FROM STORE_SETTINGS WHERE setting_key = @key)
                    UPDATE STORE_SETTINGS SET setting_value = @value WHERE setting_key = @key
                ELSE
                    INSERT INTO STORE_SETTINGS (setting_key, setting_value) VALUES (@key, @value)";
            await db.ExecuteAsync(sql, new { key, value });
        }

        public async Task SetGlobalRateAsync(string key, decimal value) => await SetSettingAsync(key, value.ToString());

        public async Task AddCommissionAsync(CommissionDisplay c)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, recorded_time, appointment_detail_id, order_detail_id)
                VALUES (@commission_type, @applied_percentage, @received_amount, @recorded_time, @appointment_detail_id, @order_detail_id)";
            await db.ExecuteAsync(sql, c);
        }

        public async Task UpdateCommissionAsync(CommissionDisplay c)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                UPDATE COMMISSION_HISTORY 
                SET commission_type = @commission_type, applied_percentage = @applied_percentage, 
                    received_amount = @received_amount, recorded_time = @recorded_time, 
                    appointment_detail_id = @appointment_detail_id, 
                    order_detail_id = @order_detail_id
                WHERE commission_id = @commission_id";
            await db.ExecuteAsync(sql, c);
        }

        public async Task DeleteCommissionAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_conn);
            await db.ExecuteAsync("DELETE FROM COMMISSION_HISTORY WHERE commission_id = @id", new { id });
        }
    }
}
