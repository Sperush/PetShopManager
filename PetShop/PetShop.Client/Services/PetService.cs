using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class PetService
    {
        private readonly string _connectionString;

        public PetService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<PetDisplay>> GetAllPetsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        p.pet_id, p.pet_name, p.species, p.breed, p.color, p.weight, 
                        p.birth_date, p.gender, p.note, p.customer_id,
                        (c.first_name + ' ' + c.last_name) AS owner_name,
                        pn.num AS owner_phone
                    FROM PET p
                    JOIN CUSTOMER c ON p.customer_id = c.customer_id
                    LEFT JOIN PHONE_NUM pn ON c.customer_id = pn.customer_id
                    ORDER BY p.pet_id DESC";

                var result = await db.QueryAsync<PetDisplay>(sql);
                return result.ToList();
            }
        }

        public async Task AddPetAsync(PetDisplay pet)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO PET (pet_name, species, breed, color, weight, birth_date, gender, note, customer_id) 
                    VALUES (@pet_name, @species, @breed, @color, @weight, @birth_date, @gender, @note, @customer_id)";
                await db.ExecuteAsync(sql, pet);
            }
        }

        public async Task UpdatePetAsync(PetDisplay pet)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE PET 
                    SET pet_name = @pet_name, species = @species, breed = @breed, color = @color, 
                        weight = @weight, birth_date = @birth_date, gender = @gender, 
                        note = @note, customer_id = @customer_id
                    WHERE pet_id = @pet_id";
                await db.ExecuteAsync(sql, pet);
            }
        }

        public async Task DeletePetAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string checkSql = "SELECT COUNT(1) FROM APPOINTMENT WHERE pet_id = @id";
                int count = await db.ExecuteScalarAsync<int>(checkSql, new { id });

                if (count > 0)
                {
                    throw new Exception("Không thể xóa thú cưng này vì đã có lịch hẹn trong quá khứ");
                }

                string sql = "DELETE FROM PET WHERE pet_id = @id";
                await db.ExecuteAsync(sql, new { id });
            }
        }
        public async Task<List<PetDisplay>> GetPetsByCustomerIdAsync(int customerId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM PET WHERE customer_id = @customerId";
            return (await db.QueryAsync<PetDisplay>(sql, new { customerId })).ToList();
        }
    }
}
