using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class EmployeeService
    {
        private readonly string _conn;
        public EmployeeService(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection");

        public async Task<List<RoleDisplay>> GetRolesAsync()
        {
            using IDbConnection db = new SqlConnection(_conn); 
            return (await db.QueryAsync<RoleDisplay>("SELECT * FROM ROLE")).ToList();
        }
        public async Task<int> AddRoleAsync(RoleDisplay role)
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"INSERT INTO ROLE (role_name) VALUES (@role_name); 
                           SELECT CAST(SCOPE_IDENTITY() as int);";
            return await db.QuerySingleAsync<int>(sql, role);
        }

        public async Task<List<EmployeeDisplay>> GetEmployeesAsync()
        {
            using IDbConnection db = new SqlConnection(_conn);
            string sql = @"
                SELECT e.*, ISNULL(r.role_name, 'N/A') as role_name, pn.num as phone 
                FROM EMPLOYEE e 
                LEFT JOIN ROLE r ON e.role_id = r.role_id 
                LEFT JOIN PHONE_NUM pn ON e.employee_id = pn.employee_id
                ORDER BY e.employee_id DESC";
            return (await db.QueryAsync<EmployeeDisplay>(sql)).ToList();
        }

        public async Task<int> AddEmployeeAsync(EmployeeDisplay emp)
        {
            using IDbConnection db = new SqlConnection(_conn);
            db.Open();
            using var trans = db.BeginTransaction();
            try
            {
                string sql = @"INSERT INTO EMPLOYEE (first_name, middle_name, last_name, address, monthly_salary, is_active, account_id, role_id) 
                               VALUES (@first_name, @middle_name, @last_name, @address, @monthly_salary, @is_active, @account_id, @role_id);
                               SELECT CAST(SCOPE_IDENTITY() as int);";
                int newId = await db.QuerySingleAsync<int>(sql, emp, transaction: trans);
                
                if (!string.IsNullOrEmpty(emp.phone))
                {
                    await db.ExecuteAsync("INSERT INTO PHONE_NUM (employee_id, num) VALUES (@newId, @phone)", new { newId, emp.phone }, transaction: trans);
                }
                
                trans.Commit();
                return newId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task UpdateEmployeeAsync(EmployeeDisplay emp)
        {
            using IDbConnection db = new SqlConnection(_conn);
            db.Open();
            using var trans = db.BeginTransaction();
            try
            {
                string sql = @"UPDATE EMPLOYEE 
                               SET first_name=@first_name, middle_name=@middle_name, last_name=@last_name, 
                                   address=@address, monthly_salary=@monthly_salary, is_active=@is_active, account_id=@account_id, role_id=@role_id
                               WHERE employee_id=@employee_id";
                await db.ExecuteAsync(sql, emp, transaction: trans);

                await db.ExecuteAsync("DELETE FROM PHONE_NUM WHERE employee_id=@employee_id", new { emp.employee_id }, transaction: trans);
                if (!string.IsNullOrEmpty(emp.phone))
                {
                    await db.ExecuteAsync("INSERT INTO PHONE_NUM (employee_id, num) VALUES (@employee_id, @phone)", new { emp.employee_id, emp.phone }, transaction: trans);
                }

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_conn))
            {
                db.Open();
                

                string checkSql = @"
                    SELECT 
                        (SELECT COUNT(1) FROM ORDERS WHERE employee_id = @id) +
                        (SELECT COUNT(1) FROM APPOINTMENT_DETAIL WHERE employee_id = @id)";
                int transactionCount = await db.ExecuteScalarAsync<int>(checkSql, new { id });

                if (transactionCount > 0)
                {

                    await db.ExecuteAsync("UPDATE EMPLOYEE SET is_active = 0 WHERE employee_id = @id", new { id });
                    return;
                }


                using (var trans = db.BeginTransaction())
                {
                    try
                    {
                        int? accountId = await db.QueryFirstOrDefaultAsync<int?>(
                            "SELECT account_id FROM EMPLOYEE WHERE employee_id = @id", 
                            new { id }, transaction: trans);

                        await db.ExecuteAsync("DELETE FROM PHONE_NUM WHERE employee_id=@id", new { id }, transaction: trans);
                        await db.ExecuteAsync("DELETE FROM WORK_SHIFT WHERE employee_id = @id", new { id }, transaction: trans);
                        await db.ExecuteAsync("DELETE FROM EMPLOYEE WHERE employee_id = @id", new { id }, transaction: trans);

                        if (accountId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM ACCOUNT WHERE account_id = @accountId", new { accountId }, transaction: trans);
                        }

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
    }
}
