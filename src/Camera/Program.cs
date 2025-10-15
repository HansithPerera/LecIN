using Backend.Api;
using Camera.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient<OpenApiClient>(client =>
{
    var baseAddress = builder.Configuration["BaseAddress"] ?? throw new InvalidOperationException("BaseAddress configuration is required");
    client.BaseAddress = new Uri(baseAddress);
    var apiKey = builder.Configuration["ApiKey"] ?? throw new InvalidOperationException("ApiKey configuration is required");
    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
});

builder.Services.AddSingleton<OpenApiClient>(provider => 
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(OpenApiClient));
    return new OpenApiClient(httpClient);
});

builder.Services.AddSingleton<FaceService>();

var host = builder.Build();


host.Run();