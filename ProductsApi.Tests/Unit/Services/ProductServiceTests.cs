using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using ProductsApi.Application.DTOs;
using ProductsApi.Application.Interfaces;
using ProductsApi.Application.Mappings;
using ProductsApi.Application.Services;
using ProductsApi.Application.Validators;
using ProductsApi.Domain.Entities;
using ProductsApi.Domain.Exceptions;

namespace ProductsApi.Tests.Unit.Services;

[TestFixture]
public class ProductServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private IMapper _mapper = null!;
    private IValidator<CreateProductRequest> _createValidator = null!;
    private IValidator<UpdateProductRequest> _updateValidator = null!;
    private ProductService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _createValidator = new CreateProductRequestValidator();
        _updateValidator = new UpdateProductRequestValidator();
        _sut = new ProductService(_uowMock.Object, _mapper, _createValidator, _updateValidator);
    }

    [Test]
    public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Nom = "Produit A", Prix = 10m, Stock = 5, Categorie = "Cat1" },
            new() { Id = 2, Nom = "Produit B", Prix = 20m, Stock = 10, Categorie = "Cat2" }
        };
        _uowMock.Setup(u => u.Products.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProductResponse()
    {
        // Arrange
        var product = new Product { Id = 1, Nom = "Produit A", Prix = 10m, Stock = 5, Categorie = "Cat1" };
        _uowMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nom, Is.EqualTo("Produit A"));
        Assert.That(result.Prix, Is.EqualTo(10m));
    }

    [Test]
    public void GetByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uowMock.Setup(u => u.Products.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.GetByIdAsync(999));
    }

    [Test]
    public async Task CreateAsync_WhenValidRequest_ShouldReturnProductResponse()
    {
        // Arrange
        var request = new CreateProductRequest("Nouveau Produit", 25.99m, 100, "Electronique");
        _uowMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 1; return p; });
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nom, Is.EqualTo("Nouveau Produit"));
        Assert.That(result.Prix, Is.EqualTo(25.99m));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WhenNomEmpty_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateProductRequest("", 10m, 5, "Cat1");

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public void CreateAsync_WhenPrixNegative_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateProductRequest("Produit", -5m, 5, "Cat1");

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public void CreateAsync_WhenStockNegative_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateProductRequest("Produit", 10m, -1, "Cat1");

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(async () => await _sut.CreateAsync(request));
    }

    [Test]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateAndSave()
    {
        // Arrange
        var product = new Product { Id = 1, Nom = "Ancien Nom", Prix = 10m, Stock = 5, Categorie = "Cat1" };
        var request = new UpdateProductRequest("Nouveau Nom", 20m, 10, "Cat2");
        _uowMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
        _uowMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        Assert.That(result.Nom, Is.EqualTo("Nouveau Nom"));
        Assert.That(result.Prix, Is.EqualTo(20m));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uowMock.Setup(u => u.Products.GetByIdAsync(999)).ReturnsAsync((Product?)null);
        var request = new UpdateProductRequest("Nom", 10m, 5, "Cat1");

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.UpdateAsync(999, request));
    }

    [Test]
    public async Task DeleteAsync_WhenProductExists_ShouldDeleteAndSave()
    {
        // Arrange
        var product = new Product { Id = 1, Nom = "A supprimer", Prix = 10m, Stock = 5, Categorie = "Cat1" };
        _uowMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
        _uowMock.Setup(u => u.Products.DeleteAsync(1)).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _uowMock.Verify(u => u.Products.DeleteAsync(1), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uowMock.Setup(u => u.Products.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.DeleteAsync(999));
    }
}
