using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Shopilent.Infrastructure.Logging.Configuration;

public static class LoggingConfiguration
{
    public static void ConfigureLogging(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(new CompactJsonFormatter(),
                Path.Combine(environment.ContentRootPath, "logs", "log-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 10 * 1024 * 1024) // 10MB
            .WriteTo.File(
                Path.Combine(environment.ContentRootPath, "logs", "error-.txt"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            // Optionally add Seq logging if configured
            .WriteTo.Conditional(
                evt => !string.IsNullOrEmpty(configuration["Seq:ServerUrl"]),
                wt => wt.Seq(configuration["Seq:ServerUrl"]!))
            .CreateLogger();

        // Clear default logging providers
        builder.Logging.ClearProviders();

        // Add Serilog
        builder.Host.UseSerilog();
    }

    public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
    {
        // Log all HTTP requests
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    diagnosticContext.Set("UserName", httpContext.User.FindFirst(ClaimTypes.Name)?.Value);
                }
            };
        });

        return app;
    }
}