using authService.Domain.Entities;
using authService.Infrastructure.Interfaces;
using authService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace authService.Infrastructure.Extensions;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("AuthServiceDbContext");

        services.AddDbContext<AuthServiceDbContext>(opt =>
            opt.UseSqlServer(conn, x => x.MigrationsAssembly("authService.Infrastructure")));

        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthServiceDbContext>()
            .AddDefaultTokenProviders();

        // Repositories DI
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}