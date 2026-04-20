using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ProductsApi.Application.DTOs;
using ProductsApi.Infrastructure.Data;

namespace ProductsApi.Tests.Integration.Controllers;

[TestFixture]
public class ProductsControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<ApiResponse<T>?> ReadResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOpts);
    }

    [Test]
    public async Task GetAll_WhenNoProducts_ShouldReturnEmptyList()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await ReadResponse<IEnumerable<ProductResponse>>(response);
        Assert.That(body!.Success, Is.True);
        Assert.That(body.Data, Is.Empty);
    }

    [Test]
    public async Task Create_WhenValidRequest_ShouldReturn201()
    {
        var request = new CreateProductRequest("Laptop", 999.99m, 10, "Electronique");
        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await ReadResponse<ProductResponse>(response);
        Assert.That(body!.Success, Is.True);
        Assert.That(body.Data!.Nom, Is.EqualTo("Laptop"));
    }

    [Test]
    public async Task Create_WhenNomEmpty_ShouldReturn400()
    {
        var request = new CreateProductRequest("", 999.99m, 10, "Electronique");
        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetById_WhenProductNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/products/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WhenProductExists_ShouldReturn204()
    {
        var created = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("A supprimer", 5m, 1, "Cat"));
        var product = (await ReadResponse<ProductResponse>(created))!.Data!;

        var response = await _client.DeleteAsync($"/api/products/{product.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Update_WhenProductExists_ShouldReturn200WithUpdatedData()
    {
        var created = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Ancien", 10m, 5, "Cat1"));
        var product = (await ReadResponse<ProductResponse>(created))!.Data!;

        var updateRequest = new UpdateProductRequest("Nouveau", 20m, 8, "Cat2");
        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updateRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await ReadResponse<ProductResponse>(response);
        Assert.That(body!.Data!.Nom, Is.EqualTo("Nouveau"));
        Assert.That(body.Data.Prix, Is.EqualTo(20m));
    }
}
