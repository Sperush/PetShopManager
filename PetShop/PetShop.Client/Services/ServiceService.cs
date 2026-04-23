using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class ServiceService
    {
        private readonly string _connectionString;

        public ServiceService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ServiceDisplay>> GetAllServicesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM SERVICE ORDER BY service_id DESC";
                var result = await db.QueryAsync<ServiceDisplay>(sql);
                return result.ToList();
            }
        }

        public async Task AddServiceAsync(ServiceDisplay service)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO SERVICE (service_name, estimated_duration, current_price) 
                    VALUES (@service_name, @estimated_duration, @current_price)";
                await db.ExecuteAsync(sql, service);
            }
        }

        public async Task UpdateServiceAsync(ServiceDisplay service)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE SERVICE 
                    SET service_name = @service_name, estimated_duration = @estimated_duration, current_price = @current_price
                    WHERE service_id = @service_id";
                await db.ExecuteAsync(sql, service);
            }
        }

        public async Task DeleteServiceAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string checkSql = "SELECT COUNT(1) FROM APPOINTMENT_DETAIL WHERE service_id = @id";
                int count = await db.ExecuteScalarAsync<int>(checkSql, new { id });

                if (count > 0)
                {
                    throw new Exception("Không thể xóa dịch vụ này vì đã có trong lịch sử lịch hẹn.");
                }

                string sql = "DELETE FROM SERVICE WHERE service_id = @id";
                await db.ExecuteAsync(sql, new { id });
            }
        }
    }
}
