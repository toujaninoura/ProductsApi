namespace ProductsApi.Application.DTOs;

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    IEnumerable<string>? Errors
)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null, null);
    public static ApiResponse<T> Fail(string message) => new(false, default, message, null);
    public static ApiResponse<T> ValidationFail(IEnumerable<string> errors)
        => new(false, default, "Validation échouée", errors);
}
