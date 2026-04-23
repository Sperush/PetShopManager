using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class AccountService
    {
        private readonly string _connectionString;

        public AccountService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<AccountDisplay?> LoginAsync(LoginRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string sqlCheck = @"
                    SELECT 
                        a.account_id, 
                        a.username, 
                        a.status,
                        ISNULL(e.is_active, 1) as is_active,
                        r.role_id,
                        r.role_name AS role, 
                        e.employee_id,
                        e.first_name + ' ' + e.last_name AS fullName
                    FROM ACCOUNT a
                    LEFT JOIN EMPLOYEE e ON a.account_id = e.account_id
                    LEFT JOIN ROLE r ON e.role_id = r.role_id
                    WHERE a.username = @Username 
                      AND a.password = @Password";

                var account = await db.QueryFirstOrDefaultAsync<AccountDisplay>(sqlCheck, request);


                if (account != null)
                {
                    string sqlUpdate = @"
                        UPDATE ACCOUNT 
                        SET last_login_time = SYSDATETIMEOFFSET() 
                        WHERE account_id = @id";
                    await db.ExecuteAsync(sqlUpdate, new { id = account.account_id });
                }

                return account;
            }
        }

        public async Task<int> CreateAccountAsync(AccountCreateRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string sqlInsert = @"
                    INSERT INTO ACCOUNT (username, password) 
                    VALUES (@username, @password);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                return await db.QuerySingleAsync<int>(sqlInsert, request);
            }
        }
    }
}
