using System.Net;
using Backend;
using Backend.Database;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class CameraTests : IClassFixture<MockAppBuilder>
{
    private readonly string _apiKey;
    private readonly MockAppBuilder _mockAppBuilder;

    public CameraTests(MockAppBuilder mockAppBuilder)
    {
        _mockAppBuilder = mockAppBuilder;
        _apiKey = SeedCamera(_mockAppBuilder.Services.GetRequiredService<AppService>());
    }

    private static string SeedCamera(AppService appService)
    {
        var camera = new Camera
        {
            Name = "Test Camera",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var apiKey = ApiKey.Create(out var key, "Camera API Key");

        appService.CreateApiKeyAsync(apiKey).GetAwaiter().GetResult();
        appService.CreateCameraAsync(camera).GetAwaiter().GetResult();
        appService.SetCameraApiKeyAsync(camera.Id, apiKey.Id, ApiKeyRole.Primary).GetAwaiter().GetResult();
        return key;
    }

    [Fact]
    public async Task TestGetCameraDetails()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, _apiKey);
        var response = await client.GetAsync("/api/Camera/details");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestUnauthorizedWhenNoApiKey()
    {
        var client = _mockAppBuilder.CreateClient();
        var response = await client.GetAsync("/api/Camera/details");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestForbiddenWhenInvalidApiKey()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, "invalid-api-key");
        var response = await client.GetAsync("/api/Camera/details");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task TestForbiddenWhenApiKeyNotCamera()
    {
        var apiKey = ApiKey.Create(out var unhashedKey, "Not a camera key");
        var appService = _mockAppBuilder.Services.GetRequiredService<AppService>();
        await appService.CreateApiKeyAsync(apiKey);
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, unhashedKey);
        var response = await client.GetAsync("/api/Camera/details");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}