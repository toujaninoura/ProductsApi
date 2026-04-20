using AutoMapper;
using ProductsApi.Application.DTOs;
using ProductsApi.Domain.Entities;

namespace ProductsApi.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<CreateProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>();
    }
}
