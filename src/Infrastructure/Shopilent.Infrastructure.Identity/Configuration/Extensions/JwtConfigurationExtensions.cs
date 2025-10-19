using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shopilent.Infrastructure.Identity.Configuration.Settings;

namespace Shopilent.Infrastructure.Identity.Configuration.Extensions;

internal static class JwtConfigurationExtensions
{
    internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind JWT settings from configuration
        var jwtSettings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(jwtSettings);

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        if (string.IsNullOrEmpty(jwtSettings.Secret))
        {
            throw new InvalidOperationException("JWT Secret key is not configured.");
        }

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}