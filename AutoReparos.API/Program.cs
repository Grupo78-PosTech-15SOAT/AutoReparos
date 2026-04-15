using AutoReparos.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAPI();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "AutoReparos API v1"));
}

app.UseHttpsRedirection();

app.Run();