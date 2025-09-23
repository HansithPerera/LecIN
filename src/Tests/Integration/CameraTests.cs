using System.Net;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using SupabaseShared.Models;
using static System.Security.Cryptography.SHA256;
using static System.Text.Encoding;
using Convert = System.Convert;

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
        var camera = new SupabaseShared.Models.Camera
        {
            Name = "Test Camera",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var key = Guid.NewGuid().ToString();
        var bas64Sha256 = Convert.ToBase64String(HashData(UTF8.GetBytes(key)));
        var apiKey = new ApiKey
        {
            Prefix = key[..8],
            Hash = bas64Sha256,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            Name = "Test API Key"
        };
        
        var cameraApiKey = new CameraApiKey
        {
            CameraId = camera.Id,
            ApiKeyId = apiKey.Id,
            Role = ApiKeyRole.Primary
        };
        var cam = client.From<SupabaseShared.Models.Camera>().Insert(camera).Result;
        var ap = client.From<ApiKey>().Insert(apiKey).Result;
        client.From<CameraApiKey>().Insert(cameraApiKey).Wait();
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