using AutoMapper;
using FluentValidation;
using ProductsApi.Application.DTOs.Categories;
using ProductsApi.Application.Interfaces;
using ProductsApi.Domain.Entities;
using ProductsApi.Domain.Exceptions;
using DomainValidationException = ProductsApi.Domain.Exceptions.ValidationException;

namespace ProductsApi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryRequest> _createValidator;

    public CategoryService(
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<CreateCategoryRequest> createValidator)
    {
        _uow = uow;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _uow.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        var category = await _uow.Categories.GetByIdWithProductsAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var category = _mapper.Map<Category>(request);
        category.CreatedAt = DateTime.UtcNow;
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);

        _mapper.Map(request, category);
        await _uow.Categories.UpdateAsync(category);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);
        await _uow.Categories.DeleteAsync(category.Id);
        await _uow.SaveChangesAsync();
    }
}
