using Microsoft.EntityFrameworkCore;
using ProductsApi.Application.Interfaces;
using ProductsApi.Domain.Entities;
using ProductsApi.Infrastructure.Data;

namespace ProductsApi.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category?> GetByIdWithProductsAsync(int id)
        => await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .ToListAsync();

    public async Task<Category?> GetByNameAsync(string name)
        => await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<Category> AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Category entity)
    {
        _context.Categories.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is not null)
            _context.Categories.Remove(category);
    }
}
