using ProductsApi.Domain.Entities;

namespace ProductsApi.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<Category?> GetByIdWithProductsAsync(int id);
}
