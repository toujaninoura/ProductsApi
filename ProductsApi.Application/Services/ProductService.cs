using AutoMapper;
using FluentValidation;
using ProductsApi.Application.DTOs;
using ProductsApi.Application.Interfaces;
using ProductsApi.Domain.Entities;
using ProductsApi.Domain.Exceptions;

namespace ProductsApi.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _uow = uow;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _uow.Products.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var product = _mapper.Map<Product>(request);
        product.CreatedAt = DateTime.UtcNow;
        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);

        _mapper.Map(request, product);
        product.UpdatedAt = DateTime.UtcNow;
        await _uow.Products.UpdateAsync(product);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);
        await _uow.Products.DeleteAsync(product.Id);
        await _uow.SaveChangesAsync();
    }
}
