using Challenge.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;

namespace Challenge.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (exception is ValidationException ve)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Validation failed",
                        errors = ve.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage })
                    });
                }
                else if (exception is KeyNotFoundException)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { message = exception!.Message });
                }
                else if (exception is InvalidOperationException)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { message = exception!.Message });
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
                }
            });
        });

        return app;
    }

    public static WebApplication UseApiOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }

    public static WebApplication EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        return app;
    }
}
