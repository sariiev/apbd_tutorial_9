using Microsoft.Data.SqlClient;
using Tutorial9.Exceptions;

namespace Tutorial9.Repositories;

public class OrderService : IOrderService
{
    private string _connectionString;

    public OrderService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentException("Connection string not found");
    }

    public async Task<int> GetProductPurchaseOrderId(int idProduct, int amount, DateTime createdAt)
    {
        string getProductPurchaseOrderIdQuery =
            "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(getProductPurchaseOrderIdQuery, connection))
        {
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);

            object? result = await command.ExecuteScalarAsync();
            if (result == null)
            {
                throw new OrderNotFoundException();
            }

            return (int) result;
        }
    }

    public async Task<bool> OrderIsCompleted(int id)
    {
        string orderIsCompletedQuery = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @id";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(orderIsCompletedQuery, connection))
        {
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@id", id);

            int count = (int) await command.ExecuteScalarAsync();
            return count > 0;
        }    
    }

    public async Task FulfillOrder(SqlConnection connection, SqlTransaction transaction, int id)
    {
        string fulfillOrderQuery = "UPDATE [Order] SET FulfilledAt = @fulfilledAt WHERE IdOrder = @id";

        using (SqlCommand command = new SqlCommand(fulfillOrderQuery, connection, transaction))
        {
            command.Parameters.AddWithValue("@fulfilledAt", DateTime.Now);
            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                throw new OrderNotFoundException();
            }
        }
    }
}