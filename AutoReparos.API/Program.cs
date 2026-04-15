using AutoReparos.API;
using AutoReparos.API.Endpoints;
using AutoReparos.Application;
using AutoReparos.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAPI();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "AutoReparos API v1"));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapClientesEndpoints();

app.Run();