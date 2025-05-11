namespace Tutorial9.Repositories;

public interface IProductService
{
    Task<bool> ProductExists(int id);

    Task<double> GetPrice(int id);
}