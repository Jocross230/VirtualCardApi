using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace VirtualCard.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path,
                    Detail = _env.IsDevelopment() ? ex.Message : null
                };

                problem.Extensions["traceId"] = traceId;

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
