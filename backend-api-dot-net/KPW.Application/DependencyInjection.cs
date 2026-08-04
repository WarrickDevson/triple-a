using System.Reflection;
using FluentValidation;
using KPW.Application.Features.Auth.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace KPW.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        return services;
    }
}
