using Microsoft.Data.SqlClient;

namespace Tutorial9.Repositories;

public interface IOrderService
{
    Task<int> GetProductPurchaseOrderId(int idProduct, int amount, DateTime createdAt);

    Task<bool> OrderIsCompleted(int id);

    Task FulfillOrder(SqlConnection connection, SqlTransaction transaction, int id);
}