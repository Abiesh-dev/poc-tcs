using Microsoft.OpenApi.Models;

namespace UserDirectory.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "User Directory API",
                Version = "v1",
                Description = "CRUD API backing the User Directory web application.",
            });
        });

        return services;
    }
}
