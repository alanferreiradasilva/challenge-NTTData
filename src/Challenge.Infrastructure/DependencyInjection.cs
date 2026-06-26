using Challenge.Application.Interfaces;
using Challenge.Domain.Interfaces;
using Challenge.Infrastructure.Data;
using Challenge.Infrastructure.Repositories;
using Challenge.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Challenge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton(jwtSettings);
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
