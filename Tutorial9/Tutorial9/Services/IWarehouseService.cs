using Tutorial9.DTO;

namespace Tutorial9.Repositories;

public interface IWarehouseService
{
    Task<int> Fulfill(FulfillOrderDTO fulfillOrderDto);
    Task<int> FulfillProcedure(FulfillOrderDTO fulfillOrderDto);
    Task<bool> WarehouseExists(int id);
}