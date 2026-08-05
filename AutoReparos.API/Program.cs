using AutoReparos.API;
using AutoReparos.API.Endpoints;
using AutoReparos.Application;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAPI(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfraestructureSwagger();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddHealthChecks();
builder.Services.AddOpenTelemetryObservability(builder.Configuration, builder.Logging);

var app = builder.Build();

await DbInitializer.SeedDataAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoReparos API v1"));
}

app.UseExceptionHandler();

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUsuariosEndpoints();
app.MapClientesEndpoints();
app.MapVeiculosEndpoints();
app.MapServicosEndpoints();
app.MapInsumosEndpoints();
app.MapOrdemServicoEndpoints();
app.MapDashboardEndpoints();

app.MapHealthChecks("/health");

app.Run();
