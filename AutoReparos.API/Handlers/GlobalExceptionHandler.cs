using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AutoReparos.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                DomainException ex => (StatusCodes.Status400BadRequest, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor.")
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new { message = message }, cancellationToken);
            return true;
        }
    }
}
