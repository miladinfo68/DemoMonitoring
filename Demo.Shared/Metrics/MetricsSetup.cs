using Microsoft.AspNetCore.Builder;

namespace Demo.Shared.Metrics
{
    public static class MetricsSetup
    {
        public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder app)
        {
            // Expose /metrics endpoint via OpenTelemetry Prometheus exporter
            app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");
            //app.UseMetricServer(); // Exposes /metrics endpoint
            //app.UseHttpMetrics();  // Tracks HTTP request metrics
            return app;
        }
    }
}