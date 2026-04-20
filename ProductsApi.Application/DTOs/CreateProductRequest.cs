namespace ProductsApi.Application.DTOs;

public record CreateProductRequest(
    string Nom,
    decimal Prix,
    int Stock,
    string Categorie
);
