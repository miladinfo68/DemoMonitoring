using Demo.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demo.Shared.Telemetry;

public static class TelemetrySetup
{
    public static IServiceCollection AddCustomTelemetry(
        this IServiceCollection services,
        IOptions<ObservabilityOptions> options)
    {
        var opts = options.Value;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(opts.ServiceName, serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = opts.Environment
                })
            )
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(opts.ServiceName)
                    .AddSource(opts.Environment)
                    .AddOtlpExporter(cfg => // For Jaeger and Collector
                    {
                        cfg.Endpoint = new Uri(opts.Telemetry.OtlpEndpoint);
                        cfg.Protocol = OtlpExportProtocol.Grpc;
                        Console.WriteLine($"XXXX===> Tracing OTLP: {cfg.Endpoint}");
                    })
                    .AddZipkinExporter(zipkin => { zipkin.Endpoint = new Uri(opts.Telemetry.ZipkinEndpoint); });

                if (opts.Telemetry.EnableConsoleExporter)
                    t.AddConsoleExporter();
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation() // Collects HTTP metrics
                    .AddOtlpExporter(cfg =>
                    {
                        cfg.Endpoint = new Uri(opts.Telemetry.OtlpEndpoint);
                        cfg.Protocol = OtlpExportProtocol.Grpc;
                        Console.WriteLine($"YYYY====> Metrics OTLP: {cfg.Endpoint}");
                    })
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(opts.ServiceName)
                    .AddMeter(opts.Environment)
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddPrometheusExporter(); // Exports metrics to /metrics
            })
            .WithLogging(logging => logging // Enable OpenTelemetry logging
                .AddOtlpExporter(cfg =>
                {
                    cfg.Endpoint =new Uri(opts.Telemetry.OtlpEndpoint);
                    cfg.Protocol = OtlpExportProtocol.Grpc;
                    Console.WriteLine($"ZZZZ====> Logging OTLP: {cfg.Endpoint}");
                }));

        return services;
    }
}