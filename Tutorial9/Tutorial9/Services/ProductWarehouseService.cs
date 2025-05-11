using Microsoft.Data.SqlClient;

namespace Tutorial9.Repositories;

public class ProductWarehouseService : IProductWarehouseService
{
    private IProductService _productService;

    public ProductWarehouseService(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<int> Insert(SqlConnection connection, SqlTransaction transaction, int idWarehouse, int idProduct, int idOrder, int amount)
    {
        string insertQuery = @"INSERT INTO Product_Warehouse
                             (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                             VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                             SELECT SCOPE_IDENTITY();";

        using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
        {
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Price", await _productService.GetPrice(idProduct) * amount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            int result = Convert.ToInt32(await command.ExecuteScalarAsync());
            return result;
        }
    }
}