namespace ProductsApi.Application.DTOs;

public record UpdateProductRequest(
    string Nom,
    decimal Prix,
    int Stock,
    string Categorie
);
