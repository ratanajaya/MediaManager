using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Serilog;

namespace CoreAPI.CustomMiddlewares;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionMiddleware(RequestDelegate next) {
        _next = next;
    }
    public async Task Invoke(HttpContext context) {
        try {
            await _next.Invoke(context);
        }
        catch(KeyNotFoundException ex) {
            await HandleExceptionAsync(context, ex, (int)HttpStatusCode.NotFound).ConfigureAwait(false);
        }
        catch(UnauthorizedAccessException ex) {
            await HandleExceptionAsync(context, ex, (int)HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }
        catch(Exception ex) {
            await HandleExceptionAsync(context, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode) {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(exception.Message);
    }
}