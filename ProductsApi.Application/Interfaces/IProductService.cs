using ProductsApi.Application.DTOs.Products;

namespace ProductsApi.Application.Interfaces;

public interface IProductService
{
    Task<PagedProductResponse> GetPagedAsync(int page, int pageSize, string? search, int? categoryId);
    Task<ProductResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
