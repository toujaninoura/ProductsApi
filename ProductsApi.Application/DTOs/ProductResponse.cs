namespace ProductsApi.Application.DTOs;

public record ProductResponse(
    int Id,
    string Nom,
    decimal Prix,
    int Stock,
    string Categorie,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
