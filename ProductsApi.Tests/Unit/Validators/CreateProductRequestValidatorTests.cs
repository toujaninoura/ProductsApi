using NUnit.Framework;
using ProductsApi.Application.DTOs.Products;
using ProductsApi.Application.Validators;

namespace ProductsApi.Tests.Unit.Validators;

[TestFixture]
public class CreateProductRequestValidatorTests
{
    private CreateProductRequestValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new CreateProductRequestValidator();

    [Test]
    public async Task Validate_WhenAllFieldsValid_ShouldBeValid()
    {
        var request = new CreateProductRequest("Laptop Pro", "High-end laptop", 999m, 50, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_WhenNameEmpty_ShouldHaveError()
    {
        var request = new CreateProductRequest("", null, 19.99m, 50, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task Validate_WhenPriceZero_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", null, 0m, 50, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Price"), Is.True);
    }

    [Test]
    public async Task Validate_WhenPriceNegative_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", null, -1m, 50, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Price"), Is.True);
    }

    [Test]
    public async Task Validate_WhenStockNegative_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", null, 10m, -1, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Stock"), Is.True);
    }

    [Test]
    public async Task Validate_WhenStockZero_ShouldBeValid()
    {
        var request = new CreateProductRequest("Produit", null, 10m, 0, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_WhenCategoryIdZero_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", null, 10m, 5, 0);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "CategoryId"), Is.True);
    }

    [Test]
    public async Task Validate_WhenNameTooLong_ShouldHaveError()
    {
        var request = new CreateProductRequest(new string('A', 201), null, 10m, 5, 1);
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }
}
