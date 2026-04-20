using ProductsApi.Domain.Entities;

namespace ProductsApi.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategorieAsync(string categorie);
}
