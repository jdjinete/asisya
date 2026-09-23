using System.Reflection;
using Asisya.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Asisya.Application;

/// <summary>
/// Provides extension methods for registering Application layer services in the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers, pipeline behaviors, and FluentValidation validators.
    /// </summary>
    /// <param name="services">The service collection descriptor.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR handlers and validation pipeline behavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
