namespace ProductsApi.Application.DTOs.Categories;

public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    int ProductCount
);
