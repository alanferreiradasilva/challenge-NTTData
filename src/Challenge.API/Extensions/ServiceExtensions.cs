using System.Text;
using Challenge.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Challenge.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddApiOpenApi(
        this IServiceCollection services)
    {
        services.AddOpenApi();

        return services;
    }
}
