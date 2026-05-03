using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AutoReparos.Infra.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;

namespace AutoReparos.IntegrationTests.Infrastructure
{
    [Collection("IntegrationTests")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected const string DefaultAdminEmail = "admin@autoreparos.com";
        protected const string DefaultAdminPassword = "Admin@123";

        protected readonly CustomWebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;
        protected readonly IServiceProvider Services;

        protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Client = Factory.CreateClient();
            Services = Factory.Services;
        }

        protected async Task AuthenticateAsync()
        {
            var loginRequest = new LoginRequestDTO(DefaultAdminEmail, DefaultAdminPassword);
            var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
            }
        }

        public async Task InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
            await DbInitializer.SeedDataAsync(Services, skipMigration: true);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
