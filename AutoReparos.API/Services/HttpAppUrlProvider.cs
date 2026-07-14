using AutoReparos.Application.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AutoReparos.API.Services
{
    public class HttpAppUrlProvider : IAppUrlProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public HttpAppUrlProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public string GetBaseUrl()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is not null)
            {
                var request = context.Request;
                
                // Em ambientes de cloud como Kubernetes/AWS, o Ingress passa o host/proto originais nos cabeçalhos X-Forwarded-*
                var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() 
                           ?? request.Host.Value;
                           
                var scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault() 
                             ?? request.Scheme;

                return $"{scheme}://{host}";
            }

            // Fallback para a variável App:BaseUrl se não estivermos no contexto HTTP (testes, migrations, seeds, console app, etc.)
            return _configuration["App:BaseUrl"] ?? "http://localhost:8080";
        }
    }
}
