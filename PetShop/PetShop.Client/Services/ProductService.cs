using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PetShop.DTO;

namespace PetShop.Services
{
    public class ProductService
    {
        private readonly string _connectionString;

        public ProductService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ProductDisplay>> GetAllProductsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM PRODUCT ORDER BY product_id DESC";
                var result = await db.QueryAsync<ProductDisplay>(sql);
                return result.ToList();
            }
        }

        public async Task AddProductAsync(ProductDisplay product)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO PRODUCT (product_name, category, pet_type_tag, stock_quantity, purchase_price, selling_price, description, is_active) 
                    VALUES (@product_name, @category, @pet_type_tag, @stock_quantity, @purchase_price, @selling_price, @description, @is_active)";
                await db.ExecuteAsync(sql, product);
            }
        }

        public async Task UpdateProductAsync(ProductDisplay product)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE PRODUCT 
                    SET product_name = @product_name, category = @category, pet_type_tag = @pet_type_tag, 
                        stock_quantity = @stock_quantity, purchase_price = @purchase_price, selling_price = @selling_price,
                        description = @description, is_active = @is_active
                    WHERE product_id = @product_id";
                await db.ExecuteAsync(sql, product);
            }
        }

        public async Task UpdateStockAsync(int id, int stockQuantity)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE PRODUCT SET stock_quantity = @stock_quantity WHERE product_id = @id";
                await db.ExecuteAsync(sql, new { id, stock_quantity = stockQuantity });
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string checkSql = "SELECT COUNT(1) FROM ORDER_DETAIL WHERE product_id = @id";
                int count = await db.ExecuteScalarAsync<int>(checkSql, new { id });
                
                if (count > 0)
                {
                    throw new Exception("Không thể xóa sản phẩm này vì đã có trong lịch sử giao dịch. Bạn nên tắt trạng thái hoạt động thay vì xóa.");
                }

                string sql = "DELETE FROM PRODUCT WHERE product_id = @id";
                await db.ExecuteAsync(sql, new { id });
            }
        }
    }
}
