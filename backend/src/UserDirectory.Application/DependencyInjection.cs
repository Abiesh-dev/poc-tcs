using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UserDirectory.Application.Abstractions;
using UserDirectory.Application.Users;

namespace UserDirectory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UserService>(ServiceLifetime.Singleton);
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
