using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial9.DTO;
using Tutorial9.Exceptions;

namespace Tutorial9.Repositories;

public class WarehouseService : IWarehouseService
{
    private string _connectionString;
    private IProductService _productService;
    private IOrderService _orderService;
    private IProductWarehouseService _productWarehouseService;

    public WarehouseService(IConfiguration configuration, IProductService productService, IOrderService orderService, IProductWarehouseService productWarehouseService)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentException("Connection string not found");
        _productService = productService;
        _orderService = orderService;
        _productWarehouseService = productWarehouseService;
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

            SqlTransaction transaction = connection.BeginTransaction();
            
            try
            {

                await _orderService.FulfillOrder(connection, transaction, orderId);

                int productWarehouseId = await _productWarehouseService.Insert(connection, transaction, fulfillOrderDto.IdWarehouse,
                    fulfillOrderDto.IdProduct, orderId, fulfillOrderDto.Amount);

                transaction.Commit();

                return productWarehouseId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                throw ex;
            }
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

    public async Task<int> FulfillProcedure(FulfillOrderDTO fulfillOrderDto)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand("AddProductToWarehouse", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdProduct", fulfillOrderDto.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", fulfillOrderDto.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", fulfillOrderDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", fulfillOrderDto.CreatedAt);
            
            await connection.OpenAsync();

            try
            {
                object? result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    int productWarehouseId = Convert.ToInt32(result);
                    return productWarehouseId;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Invalid parameter: Provided IdProduct does not exist"))
                {
                    throw new ProductNotFoundException();
                }
                if (ex.Message.Contains("Invalid parameter: There is no order to fullfill"))
                {
                    throw new OrderNotFoundException();
                }
                if (ex.Message.Contains("Invalid parameter: Provided IdWarehouse does not exist"))
                {
                    throw new WarehouseNotFoundException();
                }

                if (ex.Message.Contains("Invalid parameter: Amount must a positive integer"))
                {
                    throw new InvalidAmountException();
                }
                throw ex;
            }
        }    
    }
}