using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{

    public class OrderService
    {
        private readonly string _connectionString;

        public OrderService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task UpdateOrderAsync(OrderFullDisplay order)
        {
            using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            using var trans = db.BeginTransaction();
            try
            {

                string sqlOldDetails = "SELECT product_id, quantity FROM ORDER_DETAIL WHERE order_id = @order_id";
                var oldDetails = await db.QueryAsync(sqlOldDetails, new { order.order_id }, trans);
                foreach (var d in oldDetails)
                {
                    await db.ExecuteAsync("UPDATE PRODUCT SET stock_quantity = stock_quantity + @quantity WHERE product_id = @product_id", new { d.quantity, d.product_id }, trans);
                }


                await db.ExecuteAsync("DELETE FROM COMMISSION_HISTORY WHERE order_detail_id IN (SELECT order_detail_id FROM ORDER_DETAIL WHERE order_id = @order_id)", new { order.order_id }, trans);
                await db.ExecuteAsync("DELETE FROM ORDER_DETAIL WHERE order_id = @order_id", new { order.order_id }, trans);


                await db.ExecuteAsync("UPDATE ORDERS SET customer_id = @customer_id, payment_method = @payment_method, payment_status = @payment_status, used_reward_points = @used_reward_points WHERE order_id = @order_id", order, trans);


                string sqlInsert = @"INSERT INTO ORDER_DETAIL (order_id, product_id, quantity, price_at_purchase, note)
                                    VALUES (@order_id, @product_id, @quantity, @price_at_purchase, @note)";
                foreach (var d in order.Details)
                {
                    await db.ExecuteAsync(sqlInsert, new { order_id = order.order_id, d.product_id, d.quantity, d.price_at_purchase, d.note }, trans);
                    await db.ExecuteAsync("UPDATE PRODUCT SET stock_quantity = stock_quantity - @quantity WHERE product_id = @product_id", new { d.quantity, d.product_id }, trans);
                }

                trans.Commit();
            }
            catch { trans.Rollback(); throw; }
        }


        public async Task<List<OrderDisplay>> GetOrdersAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        o.order_id, 
                        (SELECT ISNULL(SUM(d.quantity * d.price_at_purchase), 0) FROM ORDER_DETAIL d WHERE d.order_id = o.order_id) AS total_amount, 
                        o.payment_status, o.payment_method, 
                        o.created_time, o.payment_time, o.used_reward_points, 
                        o.customer_id, o.employee_id,
                        ISNULL(c.first_name + ' ' + c.last_name, 'N/A') AS customer_name,
                        p.num AS customer_phone,
                        ISNULL(e.first_name + ' ' + e.last_name, 'N/A') AS employee_name
                    FROM ORDERS o
                    LEFT JOIN CUSTOMER c ON o.customer_id = c.customer_id
                    LEFT JOIN PHONE_NUM p ON c.customer_id = p.customer_id
                    LEFT JOIN EMPLOYEE e ON o.employee_id = e.employee_id
                    ORDER BY o.created_time DESC";

                var result = await db.QueryAsync<OrderDisplay>(sql);
                return result.ToList();
            }
        }


        public async Task<OrderFullDisplay?> GetOrderByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sqlMaster = @"
                    SELECT 
                        o.order_id, 
                        (SELECT ISNULL(SUM(d.quantity * d.price_at_purchase), 0) FROM ORDER_DETAIL d WHERE d.order_id = o.order_id) AS total_amount, 
                        o.payment_status, o.payment_method, 
                        o.created_time, o.payment_time, o.used_reward_points, 
                        o.customer_id, o.employee_id,
                        ISNULL(c.first_name + ' ' + c.last_name, 'N/A') AS customer_name,
                        ISNULL(c.address, 'N/A') AS customer_address,
                        p.num AS customer_phone,
                        ISNULL(e.first_name + ' ' + e.last_name, 'N/A') AS employee_name
                    FROM ORDERS o
                    LEFT JOIN CUSTOMER c ON o.customer_id = c.customer_id
                    LEFT JOIN PHONE_NUM p ON c.customer_id = p.customer_id
                    LEFT JOIN EMPLOYEE e ON o.employee_id = e.employee_id
                    WHERE o.order_id = @Id";

                var order = await db.QueryFirstOrDefaultAsync<OrderFullDisplay>(sqlMaster, new { Id = id });

                if (order != null)
                {
                    string sqlDetails = @"
                        SELECT 
                            d.order_detail_id,
                            d.product_id,
                            d.quantity,
                            d.price_at_purchase, 
                            d.note,
                            pr.product_name
                        FROM ORDER_DETAIL d
                        JOIN PRODUCT pr ON d.product_id = pr.product_id
                        WHERE d.order_id = @Id";

                    var details = await db.QueryAsync<OrderDetailItemDisplay>(sqlDetails, new { Id = id });
                    order.Details = details.ToList();
                }

                return order;
            }
        }

        public async Task<int> AddOrderAsync(OrderCreateRequest req)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var trans = db.BeginTransaction())
                {
                    try
                    {
                        int usedPoints = req.used_reward_points ?? 0;
                        decimal totalAmount = req.Details.Sum(d => d.quantity * d.price_at_purchase);
                        decimal finalAmount = totalAmount - usedPoints;
                        if (finalAmount < 0) finalAmount = 0;
                        int pointsEarned = (int)(finalAmount * 0.01m);


                        
                        string sqlOrder = @"
                            INSERT INTO ORDERS (customer_id, employee_id, payment_status, payment_method, used_reward_points, created_time, payment_time)
                            VALUES (@customer_id, @employee_id, @payment_status, @payment_method, @used_reward_points, SYSDATETIMEOFFSET(), NULL);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        
                        int orderId = await db.QuerySingleAsync<int>(sqlOrder, new { req.customer_id, req.employee_id, req.payment_status, req.payment_method, used_reward_points = usedPoints }, trans);

                        string sqlDetail = @"
                            INSERT INTO ORDER_DETAIL (order_id, product_id, quantity, price_at_purchase, note)
                            VALUES (@order_id, @product_id, @quantity, @price_at_purchase, @note)";
                        
                        string sqlUpdateStock = @"
                            UPDATE PRODUCT 
                            SET stock_quantity = stock_quantity - @quantity
                            WHERE product_id = @product_id;";

                        int firstDetailId = 0;
                        foreach (var d in req.Details)
                        {
                            string sqlDetailWithId = sqlDetail + "; SELECT CAST(SCOPE_IDENTITY() as int);";
                            int detailId = await db.QuerySingleAsync<int>(sqlDetailWithId, new { order_id = orderId, d.product_id, d.quantity, d.price_at_purchase, d.note }, trans);
                            if (firstDetailId == 0) firstDetailId = detailId;
                            
                            await db.ExecuteAsync(sqlUpdateStock, new { d.product_id, d.quantity }, trans);
                        }


                        

                        bool isManager = await db.ExecuteScalarAsync<bool>("SELECT 1 FROM EMPLOYEE WHERE employee_id = @id AND role_id = 1", new { id = req.employee_id }, trans);

                        if (req.payment_status == "Completed" && firstDetailId > 0 && !isManager)
                        {
                            string rateStr = await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = 'sales_comm'", null, trans);
                            decimal commRate = decimal.TryParse(rateStr, out var r) ? r : 5.0m;
                            decimal commAmount = totalAmount * (commRate / 100);

                            string sqlComm = @"
                                INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, recorded_time, order_detail_id)
                                VALUES (N'Bán hàng', @rate, @amount, SYSDATETIMEOFFSET(), @detailId)";
                            await db.ExecuteAsync(sqlComm, new { rate = commRate, amount = commAmount, detailId = firstDetailId }, trans);
                        }

                        trans.Commit();
                        return orderId;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateOrderStatusAsync(int id, string status)
        {
            using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            using var trans = db.BeginTransaction();
            try
            {
                await db.ExecuteAsync("UPDATE ORDERS SET payment_status = @status WHERE order_id = @id", new { id, status }, trans);
                
                if (status == "Completed")
                {

                    decimal totalAmount = await db.ExecuteScalarAsync<decimal>("SELECT SUM(quantity * price_at_purchase) FROM ORDER_DETAIL WHERE order_id = @id", new { id }, trans);
                    int firstDetailId = await db.ExecuteScalarAsync<int>("SELECT TOP 1 order_detail_id FROM ORDER_DETAIL WHERE order_id = @id", new { id }, trans);
                    string rateStr = await db.QueryFirstOrDefaultAsync<string>("SELECT setting_value FROM STORE_SETTINGS WHERE setting_key = 'sales_comm'", null, trans);
                    decimal commRate = decimal.TryParse(rateStr, out var r) ? r : 5.0m;

                    bool exists = await db.ExecuteScalarAsync<bool>("SELECT 1 FROM COMMISSION_HISTORY WHERE order_detail_id = @did", new { did = firstDetailId }, trans);
                    bool isManager = await db.ExecuteScalarAsync<bool>("SELECT 1 FROM ORDERS o JOIN EMPLOYEE e ON o.employee_id = e.employee_id WHERE o.order_id = @id AND e.role_id = 1", new { id }, trans);
                    
                    if (!exists && firstDetailId > 0 && !isManager)
                    {
                        decimal commAmount = totalAmount * (commRate / 100);
                        await db.ExecuteAsync(@"INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, recorded_time, order_detail_id)
                                                VALUES (N'Bán hàng', @rate, @amount, SYSDATETIMEOFFSET(), @did)", 
                                                new { rate = commRate, amount = commAmount, did = firstDetailId }, trans);
                    }
                }
                else
                {

                    string sqlDeleteComm = @"DELETE FROM COMMISSION_HISTORY 
                                           WHERE order_detail_id IN (SELECT order_detail_id FROM ORDER_DETAIL WHERE order_id = @id)";
                    await db.ExecuteAsync(sqlDeleteComm, new { id }, trans);
                }
                trans.Commit();
            }
            catch { trans.Rollback(); throw; }
        }

        public async Task DeleteOrderAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();
            using var trans = db.BeginTransaction();
            try
            {

                await db.ExecuteAsync(@"DELETE FROM COMMISSION_HISTORY 
                                        WHERE order_detail_id IN (SELECT order_detail_id FROM ORDER_DETAIL WHERE order_id = @id)", 
                                        new { id }, trans);
                
                await db.ExecuteAsync("DELETE FROM ORDER_DETAIL WHERE order_id = @id", new { id }, trans);
                await db.ExecuteAsync("DELETE FROM ORDERS WHERE order_id = @id", new { id }, trans);
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
