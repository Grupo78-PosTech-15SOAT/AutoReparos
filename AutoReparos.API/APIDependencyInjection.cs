using AutoReparos.API.Handlers;

namespace AutoReparos.API
{
    public static class APIDependencyInjection
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de API
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <returns>Collection de services com os serviços da API registrados</returns>
        public static IServiceCollection AddAPI(this IServiceCollection services)
        {
            services.AddOpenApi("v1", o =>
            {
                o.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new()
                    {
                        Title = "AutoReparos API",
                        Description = "API criada para gestão de ordens de serviços de uma oficina de mecânica.",
                        Version = "v1"
                    };
                    return Task.CompletedTask;
                });
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
    }
}
