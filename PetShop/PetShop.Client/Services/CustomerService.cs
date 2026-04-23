using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class CustomerService
    {
        private readonly string _connectionString;

        public CustomerService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }


        public class CustomerPerformanceResult
        {
            public List<CustomerDisplay> Customers { get; set; } = new();
            public long ExecutionTimeMs { get; set; }
            public string MethodName { get; set; } = "";
        }


        public async Task<CustomerPerformanceResult> GetCustomersDynamicAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        c.customer_id, c.first_name, c.last_name, c.address, c.account_id, pn.num AS phone,
                        ISNULL(ord.total_pts, 0) + ISNULL(appt.total_pts, 0) AS reward_points
                    FROM CUSTOMER c
                    OUTER APPLY (
                        SELECT 
                            SUM(CAST((totals.order_total - ISNULL(o.used_reward_points, 0)) * 0.01 AS INT)) - SUM(ISNULL(o.used_reward_points, 0)) AS total_pts
                        FROM ORDERS o
                        OUTER APPLY (
                            SELECT ISNULL(SUM(od.quantity * od.price_at_purchase), 0) AS order_total
                            FROM ORDER_DETAIL od WHERE od.order_id = o.order_id
                        ) totals
                        WHERE o.customer_id = c.customer_id
                    ) ord
                    OUTER APPLY (
                        SELECT 
                            SUM(CAST((totals.appt_total - ISNULL(a.used_reward_points, 0)) * 0.01 AS INT)) - SUM(ISNULL(a.used_reward_points, 0)) AS total_pts
                        FROM APPOINTMENT a
                        JOIN PET p ON a.pet_id = p.pet_id
                        OUTER APPLY (
                            SELECT ISNULL(SUM(ad.price_at_booking), 0) AS appt_total
                            FROM APPOINTMENT_DETAIL ad WHERE ad.appointment_id = a.appointment_id
                        ) totals
                        WHERE p.customer_id = c.customer_id
                    ) appt
                    LEFT JOIN PHONE_NUM pn ON c.customer_id = pn.customer_id
                    ORDER BY c.customer_id DESC";

                var result = await db.QueryAsync<CustomerDisplay>(sql);
                sw.Stop();
                return new CustomerPerformanceResult { 
                    Customers = result.ToList(), 
                    ExecutionTimeMs = sw.ElapsedMilliseconds,
                    MethodName = "Tính toán động"
                };
            }
        }


        public async Task<CustomerPerformanceResult> GetCustomersStoredAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try {
                    string sql = @"
                        SELECT c.customer_id, c.first_name, c.last_name, c.address, c.account_id, c.reward_points, p.num AS phone 
                        FROM CUSTOMER c
                        LEFT JOIN PHONE_NUM p ON c.customer_id = p.customer_id
                        ORDER BY c.customer_id DESC";
                    var result = await db.QueryAsync<CustomerDisplay>(sql);
                    sw.Stop();
                    return new CustomerPerformanceResult { 
                        Customers = result.ToList(), 
                        ExecutionTimeMs = sw.ElapsedMilliseconds,
                        MethodName = "Đọc trực tiếp"
                    };
                } catch {
                    sw.Stop();
                    return new CustomerPerformanceResult { MethodName = "Đọc trực tiếp (Lỗi: Cột không tồn tại)", ExecutionTimeMs = -1 };
                }
            }
        }


        public async Task<List<CustomerDisplay>> GetAllCustomersAsync() => (await GetCustomersDynamicAsync()).Customers;


        public async Task AddCustomerAsync(CustomerDisplay customer)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {

                        string sqlCustomer = @"
                            INSERT INTO CUSTOMER (first_name, last_name, address, account_id) 
                            VALUES (@first_name, @last_name, @address, @account_id);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        int newCustomerId = await db.QuerySingleAsync<int>(sqlCustomer, customer, transaction);


                        if (!string.IsNullOrEmpty(customer.phone))
                        {
                            string sqlPhone = "INSERT INTO PHONE_NUM (num, customer_id) VALUES (@phone, @customer_id)";
                            await db.ExecuteAsync(sqlPhone, new { phone = customer.phone, customer_id = newCustomerId }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public async Task UpdateCustomerAsync(CustomerDisplay customer)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {

                        string sqlCustomer = @"
                            UPDATE CUSTOMER 
                            SET first_name = @first_name, last_name = @last_name, 
                                address = @address, account_id = @account_id
                            WHERE customer_id = @customer_id";
                        await db.ExecuteAsync(sqlCustomer, customer, transaction);


                        if (!string.IsNullOrEmpty(customer.phone))
                        {
                            string sqlPhone = @"
                                IF EXISTS (SELECT 1 FROM PHONE_NUM WHERE customer_id = @customer_id)
                                    UPDATE PHONE_NUM SET num = @phone WHERE customer_id = @customer_id;
                                ELSE
                                    INSERT INTO PHONE_NUM (num, customer_id) VALUES (@phone, @customer_id);";

                            await db.ExecuteAsync(sqlPhone, customer, transaction);
                        }
                        else
                        {

                            string sqlDeletePhone = "DELETE FROM PHONE_NUM WHERE customer_id = @customer_id";
                            await db.ExecuteAsync(sqlDeletePhone, new { customer_id = customer.customer_id }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public async Task DeleteCustomerAsync(int id)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                

                string checkSql = @"
                    SELECT 
                        (SELECT COUNT(1) FROM ORDERS WHERE customer_id = @id) +
                        (SELECT COUNT(1) FROM APPOINTMENT a JOIN PET p ON a.pet_id = p.pet_id WHERE p.customer_id = @id)";
                int transactionCount = await db.ExecuteScalarAsync<int>(checkSql, new { id });

                if (transactionCount > 0)
                {
                    throw new Exception("Không thể xóa khách hàng này vì đã có lịch sử giao dịch. Để đảm bảo tính chính xác của báo cáo tài chính, dữ liệu giao dịch cần được giữ lại.");
                }

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        int? accountId = await db.QueryFirstOrDefaultAsync<int?>(
                            "SELECT account_id FROM CUSTOMER WHERE customer_id = @Id", 
                            new { Id = id }, transaction);

                        await db.ExecuteAsync("DELETE FROM PHONE_NUM WHERE customer_id = @Id", new { Id = id }, transaction);
                        await db.ExecuteAsync("DELETE FROM PET WHERE customer_id = @Id", new { Id = id }, transaction);
                        await db.ExecuteAsync("DELETE FROM CUSTOMER WHERE customer_id = @Id", new { Id = id }, transaction);

                        if (accountId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM ACCOUNT WHERE account_id = @accountId", new { accountId }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<CustomerDisplay?> GetCustomerByAccountIdAsync(int accountId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            string sql = @"
                SELECT c.*, p.num AS phone 
                FROM CUSTOMER c 
                LEFT JOIN PHONE_NUM p ON c.customer_id = p.customer_id
                WHERE c.account_id = @accountId";
            return await db.QueryFirstOrDefaultAsync<CustomerDisplay>(sql, new { accountId });
        }

        public async Task RegisterCustomerAsync(CustomerRegisterRequest req)
        {
            using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            using var trans = db.BeginTransaction();
            try
            {

                string sqlAcc = "INSERT INTO ACCOUNT (username, password) VALUES (@Username, @Password); SELECT CAST(SCOPE_IDENTITY() as int);";
                int accId = await db.QuerySingleAsync<int>(sqlAcc, new { Username = req.Phone, Password = req.Password }, trans);


                string sqlCust = @"INSERT INTO CUSTOMER (first_name, last_name, account_id) 
                                   VALUES (@FirstName, @LastName, @accId);
                                   SELECT CAST(SCOPE_IDENTITY() as int);";
                int custId = await db.QuerySingleAsync<int>(sqlCust, new { req.FirstName, req.LastName, accId }, trans);


                string sqlPhone = "INSERT INTO PHONE_NUM (num, customer_id) VALUES (@Phone, @custId)";
                await db.ExecuteAsync(sqlPhone, new { Phone = req.Phone, custId }, trans);

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }
}
