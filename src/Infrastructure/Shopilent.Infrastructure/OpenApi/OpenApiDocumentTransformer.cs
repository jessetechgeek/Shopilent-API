using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Shopilent.Infrastructure.OpenApi;

internal sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string BearerScheme = "Bearer";
    private const string AuthPath = "/auth/";
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly OpenApiSecurityScheme _bearerSecurityScheme;
    private readonly OpenApiSecurityRequirement _securityRequirement;

    public OpenApiDocumentTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;

        _bearerSecurityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "Json Web Token",
            Reference = new OpenApiReference
            {
                Id = BearerScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        _securityRequirement = new OpenApiSecurityRequirement
        {
            [_bearerSecurityScheme] = Array.Empty<string>()
        };
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken = default)
    {
        var version = context.DocumentName.ToLower();
        var hasBearerScheme = (await _authenticationSchemeProvider.GetAllSchemesAsync())
            .Any(scheme => scheme.Name == BearerScheme);

        if (hasBearerScheme)
        {
            SetupSecurityScheme(document);
            await TransformPaths(document, version);
        }

        UpdateDocumentInfo(document, version);
    }

    private void SetupSecurityScheme(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
            [BearerScheme] = _bearerSecurityScheme
        };
    }

    private async Task TransformPaths(OpenApiDocument document, string version)
    {
        var versionedPaths = document.Paths
            .Where(path => path.Key.Contains($"/{version}/"))
            .Select(path =>
            {
                var pathItem = path.Value;
                foreach (var operation in pathItem.Operations)
                {
                    operation.Value.Security.Clear();

                    if (!path.Key.Contains(AuthPath, StringComparison.OrdinalIgnoreCase))
                    {
                        operation.Value.Security.Add(_securityRequirement);
                    }
                }

                return (path.Key, pathItem);
            })
            .ToDictionary(x => x.Key, x => x.pathItem);

        document.Paths.Clear();
        foreach (var (path, item) in versionedPaths)
        {
            document.Paths.Add(path, item);
        }
    }

    private static void UpdateDocumentInfo(OpenApiDocument document, string version)
    {
        var versionNumber = version.TrimStart('v');

        document.Info.Title = $"Shopilent API {version.ToUpper()}";
        document.Info.Version = $"{versionNumber}.0";
        document.Info.Description = $"Version {versionNumber} of the API";
    }
}