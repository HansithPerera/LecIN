using System.Net;
using Backend;
using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class CameraTests : IClassFixture<MockAppBuilder>
{
    private readonly string _apiKey;
    private readonly MockAppBuilder _mockAppBuilder;

    public CameraTests(MockAppBuilder mockAppBuilder)
    {
        _mockAppBuilder = mockAppBuilder;
        _apiKey = SeedCamera(_mockAppBuilder.Services.GetRequiredService<IDbContextFactory<AppDbContext>>());
    }

    private static string SeedCamera(IDbContextFactory<AppDbContext> context)
    {
        using var ctx = context.CreateDbContext();
        
        var camera = new Camera
        {
            Name = "Test Camera",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var apiKey = ApiKey.Create(out var key, "Camera API Key");

        ctx.Cameras.Add(camera);
        ctx.ApiKeys.Add(apiKey);
        ctx.CameraApiKeys.Add(new CameraApiKey
        {
            Role = ApiKeyRole.Primary,
            CameraId = camera.Id,
            ApiKeyId = apiKey.Id
        });
        ctx.SaveChanges();
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
}