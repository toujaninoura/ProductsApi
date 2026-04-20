using Microsoft.EntityFrameworkCore;
using ProductsApi.Application.Interfaces;
using ProductsApi.Domain.Entities;
using ProductsApi.Infrastructure.Data;

namespace ProductsApi.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.FindAsync(id);

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products.ToListAsync();

    public async Task<Product> AddAsync(Product entity)
    {
        await _context.Products.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Product entity)
    {
        _context.Products.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
            _context.Products.Remove(product);
    }

    public async Task<IEnumerable<Product>> GetByCategorieAsync(string categorie)
        => await _context.Products
            .Where(p => p.Categorie == categorie)
            .ToListAsync();
}
