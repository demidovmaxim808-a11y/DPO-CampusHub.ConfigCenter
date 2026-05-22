using Microsoft.Extensions.Options;
using CampusHub.ConfigCenter.Models;

namespace CampusHub.ConfigCenter.Middleware;

public class PortalHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PortalOptions _options;

    public PortalHeaderMiddleware(RequestDelegate next, IOptions<PortalOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Portal-Title", _options.Title);
        context.Response.Headers.Append("X-Portal-Semester", _options.Semester);
        await _next(context);
    }
}