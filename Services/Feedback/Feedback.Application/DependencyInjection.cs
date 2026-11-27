using System.Reflection;
using Feedback.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Feedback.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = configuration["LuckyPenny:LicenseKey"];

            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddOpenBehavior(
                typeof(ExceptionHandlingBehavior<,>));

            cfg.AddOpenBehavior(
                typeof(LoggingBehavior<,>));

            cfg.AddOpenBehavior(
                typeof(ValidationBehavior<,>));

            cfg.AddOpenBehavior(
                typeof(PerformanceBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}