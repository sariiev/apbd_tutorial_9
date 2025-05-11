using Microsoft.Data.SqlClient;
using Tutorial9.Exceptions;

namespace Tutorial9.Repositories;

public class ProductService : IProductService
{
    private string _connectionString;

    public ProductService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentException("Connection string not found");
    }
    
    public async Task<bool> ProductExists(int id)
    {
        string productExistsQuery = "SELECT COUNT(*) FROM Product WHERE IdProduct = @id";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(productExistsQuery, connection))
        {
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@id", id);

            int count = (int) await command.ExecuteScalarAsync();
            return count > 0;
        }
    }

    public async Task<double> GetPrice(int id)
    {
        string productPriceQuery = "SELECT Price FROM Product WHERE IdProduct = @id";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(productPriceQuery, connection))
        {
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@id", id);

            object? result =  await command.ExecuteScalarAsync();
            if (result == null)
            {
                throw new ProductNotFoundException();
            }

            return Convert.ToDouble(result);
        }
    }
}