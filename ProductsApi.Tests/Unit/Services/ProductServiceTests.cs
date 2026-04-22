using AutoMapper;
using FluentValidation;
using Moq;
using NUnit.Framework;
using ProductsApi.Application.DTOs;
using ProductsApi.Application.DTOs.Products;
using ProductsApi.Application.Interfaces;
using ProductsApi.Application.Mappings;
using ProductsApi.Application.Services;
using ProductsApi.Application.Validators;
using ProductsApi.Domain.Entities;
using ProductsApi.Domain.Exceptions;
using DomainValidationException = ProductsApi.Domain.Exceptions.ValidationException;

namespace ProductsApi.Tests.Unit.Services;

[TestFixture]
public class ProductServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IProductRepository> _productRepoMock = null!;
    private IMapper _mapper = null!;
    private IValidator<CreateProductRequest> _createValidator = null!;
    private IValidator<UpdateProductRequest> _updateValidator = null!;
    private ProductService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _createValidator = new CreateProductRequestValidator();
        _updateValidator = new UpdateProductRequestValidator();
        _sut = new ProductService(_uowMock.Object, _mapper, _createValidator, _updateValidator);
    }

    private static Category MakeCategory(int id = 1, string name = "Electronics") =>
        new() { Id = id, Name = name };

    private static Product MakeProduct(int id = 1, string name = "Laptop", decimal price = 999m, int stock = 10, int categoryId = 1) =>
        new() { Id = id, Name = name, Price = price, Stock = stock, CategoryId = categoryId, Category = MakeCategory() };

    [Test]
    public async Task GetPagedAsync_WhenProductsExist_ShouldReturnPagedResponse()
    {
        var products = new List<Product> { MakeProduct(1), MakeProduct(2, "Mouse", 29m) };
        _productRepoMock.Setup(r => r.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync((products, 2));

        var result = await _sut.GetPagedAsync(1, 10, null, null);

        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProductResponse()
    {
        var product = MakeProduct(1, "Laptop Pro", 1299m);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Laptop Pro"));
        Assert.That(result.Price, Is.EqualTo(1299m));
    }

    [Test]
    public void GetByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.GetByIdAsync(999));
    }

    [Test]
    public async Task CreateAsync_WhenValidRequest_ShouldReturnProductResponse()
    {
        var request = new CreateProductRequest("Laptop Pro", "High-end laptop", 1299m, 25, 1);
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 1; return p; });
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(MakeProduct(1, "Laptop Pro", 1299m));
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Laptop Pro"));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WhenNameEmpty_ShouldThrowValidationException()
    {
        var request = new CreateProductRequest("", null, 10m, 5, 1);

        Assert.ThrowsAsync<DomainValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public void CreateAsync_WhenPriceNegative_ShouldThrowValidationException()
    {
        var request = new CreateProductRequest("Produit", null, -5m, 5, 1);

        Assert.ThrowsAsync<DomainValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public void CreateAsync_WhenStockNegative_ShouldThrowValidationException()
    {
        var request = new CreateProductRequest("Produit", null, 10m, -1, 1);

        Assert.ThrowsAsync<DomainValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateAndSave()
    {
        var existing = MakeProduct(1, "Old Name", 10m);
        var request = new UpdateProductRequest("New Name", "Updated desc", 20m, 15, 1);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var updated = MakeProduct(1, "New Name", 20m);
        _productRepoMock.SetupSequence(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(1, request);

        Assert.That(result.Name, Is.EqualTo("New Name"));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);
        var request = new UpdateProductRequest("Name", null, 10m, 5, 1);

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.UpdateAsync(999, request));
    }

    [Test]
    public async Task DeleteAsync_WhenProductExists_ShouldDeleteAndSave()
    {
        var product = MakeProduct(1);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteAsync(1);

        _productRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.DeleteAsync(999));
    }
}
