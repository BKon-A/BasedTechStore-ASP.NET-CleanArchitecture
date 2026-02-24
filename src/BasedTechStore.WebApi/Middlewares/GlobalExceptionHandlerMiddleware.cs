using BasedTechStore.Common.Models.Api;
using BasedTechStore.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace BasedTechStore.WebApi.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;
            IEnumerable<string>? errors = null;

            if (exception is DomainException domainException)
            {
                statusCode = domainException.StatusCode;
                message = domainException.Message;

                if (exception is ValidationException validationException)
                {
                    errors = validationException.Errors?.SelectMany(e => e.Value);
                }
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "An internal server error occured";
                _logger.LogError(exception, "Unhandled exception occured");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var resourse = new ApiResponse<object>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new[] { message },
                StatusCode = (int)statusCode,
            };

            var json = JsonSerializer.Serialize(resourse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
