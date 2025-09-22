using System.Net;
using Backend;
using Backend.Api.Models;
using Backend.Database;
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
        _apiKey = SeedCamera(_mockAppBuilder.Services.GetRequiredService<Supabase.Client>());
    }

    private static string SeedCamera(Supabase.Client client)
    {
        var camera = new Backend.Api.Models.Camera
        {
            Name = "Test Camera",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var apiKey = new ApiKey
        {
            Prefix = "test-prefix",
            Hash = "test-hash",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            Name = "Test API Key"
        };
        client.From<Backend.Api.Camera>().Insert(camera).Wait();
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