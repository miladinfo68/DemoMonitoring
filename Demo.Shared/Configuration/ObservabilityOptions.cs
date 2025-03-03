namespace Demo.Shared.Configuration;

public class ObservabilityOptions
{
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public LoggingOptions Logging { get; set; } = new();
    public TelemetryOptions Telemetry { get; set; } = new();
}

public class LoggingOptions
{
    public string LokiUri { get; set; } = "http://loki:3100";
    public string MinimumLevel { get; set; } = "Information";
}

public class TelemetryOptions
{
    // public string JaegerHost { get; set; } = "jaeger";
    // public int JaegerPort { get; set; } = 6831;
    public string OtlpEndpoint { get; set; } = "http://otel-collector:4317";
    public string ZipkinEndpoint { get; set; } = "http://zipkin:9411/api/v2/spans";
    public bool EnableConsoleExporter { get; set; } = false; // Toggle for dev
}