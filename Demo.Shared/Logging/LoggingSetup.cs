using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.SystemConsole.Themes;

namespace Demo.Shared.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Configuration;

public static class LoggingSetup
{
    public static ILoggingBuilder AddCustomLogging(
        this ILoggingBuilder builder,
        IConfiguration configuration,
        IOptions<ObservabilityOptions> options)
    {
        var opts = options.Value;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", opts.ServiceName)
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(opts.Logging.MinimumLevel))
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Literate, // Colorful console logs
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}") // custom template
            .WriteTo.GrafanaLoki(
                uri: opts.Logging.LokiUri,
                labels: new List<LokiLabel>() { new() { Key = "app", Value = opts.ServiceName } })
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.ClearProviders().AddSerilog(dispose: true);
        return builder;
    }
    
    public static void ConfigureLogging(IConfiguration configuration, string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(configuration["Serilog:MinimumLevel"] ?? "Information"))
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate) // Colorful console logs
            .WriteTo.GrafanaLoki(
                configuration["Observability:Logging:LokiUri"] ?? "http://loki:3100",
                labels: new List<LokiLabel>() { new() { Key = "app", Value = serviceName } })
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}