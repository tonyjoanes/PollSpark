using Microsoft.AspNetCore.Http;

namespace PollSpark.Extensions;

public static class HttpResultsExtensions
{
    public static IResult TooManyRequests(string? message = null)
    {
        var response = new
        {
            Status = 429,
            Message = message ?? "Too many requests. Please try again later.",
        };

        return Results
            .Json(
                response,
                statusCode: StatusCodes.Status429TooManyRequests,
                contentType: "application/json",
                options: new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                }
            )
            .AddHeader("Retry-After", "60");
    }
}

public static class ResultExtensions
{
    public static IResult AddHeader(this IResult result, string name, string value)
    {
        return new HeaderResult(result, name, value);
    }
}

public class HeaderResult : IResult
{
    private readonly IResult _result;
    private readonly string _name;
    private readonly string _value;

    public HeaderResult(IResult result, string name, string value)
    {
        _result = result;
        _name = name;
        _value = value;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append(_name, _value);
        await _result.ExecuteAsync(httpContext);
    }
}
