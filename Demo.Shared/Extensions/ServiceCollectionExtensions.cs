using System.ComponentModel.DataAnnotations;
using Demo.Shared.Configuration;
using Demo.Shared.HealthChecks;
using Demo.Shared.Logging;
using Demo.Shared.Metrics;
using Demo.Shared.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;


namespace Demo.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ObservabilityOptions>(opts =>
        {
            if (configuration?.GetSection("Observability") is null)
            {
                throw new ValidationException("Observability must be set in appsettings.json file");
            }
            opts.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"; 
            configuration.GetSection("Observability").Bind(opts);
        });

        //LoggingSetup.ConfigureLogging(configuration, serviceName);
        //services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        services.AddLogging(builder => builder.AddCustomLogging(configuration, services.BuildServiceProvider().GetRequiredService<IOptions<ObservabilityOptions>>()));
        services.AddCustomTelemetry(services.BuildServiceProvider().GetRequiredService<IOptions<ObservabilityOptions>>());
        services.AddCustomHealthChecks();

        return services;
    }

    public static IApplicationBuilder UseSharedInfrastructure(this IApplicationBuilder app)
    {
        app.UseCustomMetrics();
        app.UseCustomHealthChecks();
        return app;
    }
}