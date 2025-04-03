using System.Text.Json;
using FastEndpoints;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // Register API assembly
});

builder.Services.AddFastEndpoints();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(
    config =>
    {
        config.Endpoints.RoutePrefix = "api";
        config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
);

app.Run();