using Microsoft.Data.SqlClient;
using Tutorial9.DTO;
using Tutorial9.Exceptions;

namespace Tutorial9.Repositories;

public class WarehouseService : IWarehouseService
{
    private string _connectionString;
    private IProductService _productService;
    private IOrderService _orderService;

    public WarehouseService(IConfiguration configuration, IProductService productService, IOrderService orderService)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentException("Connection string not found");
        _productService = productService;
        _orderService = orderService;
    }

    public async Task<int> Fulfill(FulfillOrderDTO fulfillOrderDto)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            if (fulfillOrderDto.Amount <= 0)
            {
                throw new InvalidAmountException();
            }
            
            if (! await _productService.ProductExists(fulfillOrderDto.IdProduct))
            {
                throw new ProductNotFoundException();
            }

            if (! await WarehouseExists(fulfillOrderDto.IdWarehouse))
            {
                throw new WarehouseNotFoundException();
            }

            int orderId;
            try
            {
                orderId = await _orderService.GetProductPurchaseOrderId(fulfillOrderDto.IdProduct,
                    fulfillOrderDto.Amount, fulfillOrderDto.CreatedAt);
            }
            catch (OrderNotFoundException ex)
            {
                throw ex;
            }

            if (await _orderService.OrderIsCompleted(orderId))
            {
                throw new OrderAlreadyCompletedException();
            }

            try
            {
                SqlTransaction transaction = connection.BeginTransaction();

                _orderService.FulfillOrder(connection, transaction, orderId);
            } catch 
        }
    }
    
    public async Task<bool> WarehouseExists(int id)
    {
        string warehouseExistsQuery = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @id";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(warehouseExistsQuery, connection))
        {
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@id", id);

            int count = (int) await command.ExecuteScalarAsync();
            return count > 0;
        }
    }
}