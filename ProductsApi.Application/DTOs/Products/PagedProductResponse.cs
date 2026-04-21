namespace ProductsApi.Application.DTOs.Products;

public record PagedProductResponse(
    IEnumerable<ProductResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
