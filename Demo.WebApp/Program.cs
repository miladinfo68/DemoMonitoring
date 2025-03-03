using Demo.Shared.Extensions;
using Demo.Shared.Models;
using Demo.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedInfrastructure(builder.Configuration);

var todoApiBaseUrl = builder.Configuration["TodoApiBaseUrl"]!;
builder.Services.AddHttpClient("TodoApi", client =>
{
    client.BaseAddress = new Uri(todoApiBaseUrl);
    //client.Timeout = TimeSpan.FromSeconds(60);
});
    //.AddHttpMessageHandler(sp => new LoggingHandler(sp.GetRequiredService<ILogger<LoggingHandler>>()));
builder.Services.AddScoped<TodoService>();


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSharedInfrastructure();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


// public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
// {
//     protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//     {
//         logger.LogInformation("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);
//         var response = await base.SendAsync(request, cancellationToken);
//         logger.LogInformation("Received HTTP response: {StatusCode}", response.StatusCode);
//         return response;
//     }
// }

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);

        HttpResponseMessage? response = null;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
            _logger.LogInformation("Received HTTP response: {StatusCode}", response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Method} {Uri}", request.Method, request.RequestUri);
            throw; // Re-throw to propagate the error
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "HTTP request cancelled: {Method} {Uri}", request.Method, request.RequestUri);
            throw; // Re-throw for timeout or cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during HTTP request: {Method} {Uri}", request.Method,
                request.RequestUri);
            throw; // Re-throw unexpected errors
        }

        return response ?? new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Response was null due to an internal error.")
        }; // Fallback if response is still null
    }
}


public class TodoService(IHttpClientFactory httpClientFactory, ILogger<TodoService> logger)
{
    public async Task<List<Todo>> GetTodosAsync()
    {
        var client = httpClientFactory.CreateClient("TodoApi");
        logger.LogInformation("Attempting to fetch todos from {BaseAddress}/todos", client.BaseAddress);

        try
        {
            var response = await client.GetAsync("/todos");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Raw response content: {Content}", content);

            var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
            logger.LogInformation("Fetched {Count} todos from API", todos?.Count ?? 0);
            return todos ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching todos from {BaseAddress}/todos", client.BaseAddress);
            return [];
        }
    }
}

