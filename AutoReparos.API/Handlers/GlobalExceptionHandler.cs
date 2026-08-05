using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AutoReparos.API.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                DomainException ex => (StatusCodes.Status400BadRequest, ex.Message),
                BadHttpRequestException ex => (ex.StatusCode, ex.Message),
                ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Ocorreu um erro interno inesperado.");
            }
            else
            {
                _logger.LogWarning("Exceção de negócio tratada ({StatusCode}): {Message}", statusCode, exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new { message = message }, cancellationToken);
            return true;
        }
    }
}
