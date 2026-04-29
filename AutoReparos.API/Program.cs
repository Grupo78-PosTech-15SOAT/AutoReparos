using AutoReparos.API;
using AutoReparos.API.Endpoints;
using AutoReparos.Application;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.IoC;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAPI(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfraestructureSwagger();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        await DbInitializer.SeedAsync(userManager, configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular o banco de dados.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoReparos API v1"));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUsuariosEndpoints();
app.MapClientesEndpoints();
app.MapVeiculosEndpoints();
app.MapServicosEndpoints();
app.MapInsumosEndpoints();
app.MapOrdensServicoEndpoints();

app.Run();