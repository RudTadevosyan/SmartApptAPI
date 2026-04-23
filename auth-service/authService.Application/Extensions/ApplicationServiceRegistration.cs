using authService.Application.Interfaces;
using authService.Application.Services;
using authService.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace authService.Application.Extensions;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Application services DI
        services.AddScoped<IAuthJwtService, AuthJwtService>();

        // Call Infrastructure registration internally
        services.AddInfrastructureServices(configuration);

        return services;
    }
    
}