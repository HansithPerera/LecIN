using System.Net;
using System.Net.Http.Headers;
using Backend.Database;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class AuthIntegrationTests : IClassFixture<MockAppBuilder>
{
    private readonly MockAppBuilder _mockAppBuilder;

    public AuthIntegrationTests(MockAppBuilder mockAppBuilder)
    {
        _mockAppBuilder = mockAppBuilder;
        var dataService = _mockAppBuilder.Services.GetRequiredService<Repository>();
        SeedTeacher(dataService);
    }

    private static readonly Guid TeacherId = Guid.NewGuid();
    
    private static void SeedTeacher(Repository repository)
    {
        var teacher = new Teacher
        {
            Id = TeacherId,
            FirstName = "Alice",
            LastName = "Smith",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        repository.AddTeacherAsync(teacher).GetAwaiter().GetResult();
    }


    [Fact]
    public async Task TestUnauthorizedWhenNoCredentials()
    {
        var client = _mockAppBuilder.CreateClient();
        var response = await client.GetAsync("/api/Teacher/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestForbiddenWhenNotTeacher()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "student-1");
        var response = await client.GetAsync("/api/Teacher/profile");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TestSuccessWhenTeacher()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TeacherId.ToString());
        var response = await client.GetAsync("/api/Teacher/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}