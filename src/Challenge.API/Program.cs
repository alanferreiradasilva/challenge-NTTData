using Challenge.API.Extensions;
using Challenge.Application;
using Challenge.Infrastructure;
using Challenge.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not configured.");

builder.Services
    .AddApplication()
    .AddInfrastructure(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        jwtSettings)
    .AddApiAuthentication(jwtSettings)
    .AddApiOpenApi()
    .AddControllers();

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();

app.EnsureDatabaseCreated()
   .UseApiExceptionHandler()
   .UseApiOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();