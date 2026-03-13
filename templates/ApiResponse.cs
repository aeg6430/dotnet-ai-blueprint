namespace Project.Api.Models;

/// <summary>
/// Unified API response wrapper — all endpoints must return this type.
/// Never return raw DTOs or entities directly from Controllers.
/// </summary>
public class ApiResponse<T>
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = 200)
        => new() { StatusCode = statusCode, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new() { StatusCode = statusCode, Message = message };
}
