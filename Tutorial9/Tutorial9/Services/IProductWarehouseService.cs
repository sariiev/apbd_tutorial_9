using Microsoft.Data.SqlClient;

namespace Tutorial9.Repositories;

public interface IProductWarehouseService
{
    Task<int> Insert(SqlConnection connection, SqlTransaction transaction, int idWarehouse, int idProduct, int idOrder,
        int amount);
}