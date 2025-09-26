using Backend.Api;
using Camera.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient<OpenApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddress"]);
    client.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration["ApiKey"]);
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