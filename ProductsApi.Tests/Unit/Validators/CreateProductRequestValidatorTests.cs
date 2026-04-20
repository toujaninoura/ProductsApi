using NUnit.Framework;
using ProductsApi.Application.DTOs;
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
        var request = new CreateProductRequest("Produit Test", 19.99m, 50, "Electronique");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_WhenNomEmpty_ShouldHaveError()
    {
        var request = new CreateProductRequest("", 19.99m, 50, "Electronique");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Nom"), Is.True);
    }

    [Test]
    public async Task Validate_WhenPrixZero_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", 0m, 50, "Cat");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Prix"), Is.True);
    }

    [Test]
    public async Task Validate_WhenPrixNegative_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", -1m, 50, "Cat");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Prix"), Is.True);
    }

    [Test]
    public async Task Validate_WhenStockNegative_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", 10m, -1, "Cat");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Stock"), Is.True);
    }

    [Test]
    public async Task Validate_WhenStockZero_ShouldBeValid()
    {
        var request = new CreateProductRequest("Produit", 10m, 0, "Cat");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_WhenCategorieEmpty_ShouldHaveError()
    {
        var request = new CreateProductRequest("Produit", 10m, 5, "");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Categorie"), Is.True);
    }

    [Test]
    public async Task Validate_WhenNomTooLong_ShouldHaveError()
    {
        var request = new CreateProductRequest(new string('A', 201), 10m, 5, "Cat");
        var result = await _sut.ValidateAsync(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Nom"), Is.True);
    }
}
