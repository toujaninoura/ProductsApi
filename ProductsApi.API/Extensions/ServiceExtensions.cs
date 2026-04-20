using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Application.Interfaces;
using ProductsApi.Application.Mappings;
using ProductsApi.Application.Services;
using ProductsApi.Application.Validators;
using ProductsApi.Infrastructure.Data;
using ProductsApi.Infrastructure.Repositories;

namespace ProductsApi.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ProductsDb"));
        return services;
    }
}
