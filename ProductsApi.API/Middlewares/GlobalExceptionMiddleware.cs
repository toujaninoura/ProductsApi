using FluentValidation;
using ProductsApi.Application.DTOs;
using ProductsApi.Domain.Exceptions;

namespace ProductsApi.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Ressource introuvable");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erreur de validation");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.ValidationFail(ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Erreur interne du serveur."));
        }
    }
}
