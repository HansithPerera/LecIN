using System.Net;
using System.Net.Http.Headers;
using Backend;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Tests.Integration;

public class AuthIntegrationTests : IClassFixture<MockAppBuilder>
{
    private readonly MockAppBuilder _mockAppBuilder;

    public AuthIntegrationTests(MockAppBuilder mockAppBuilder)
    {
        _mockAppBuilder = mockAppBuilder;
        var e = _mockAppBuilder.Services.GetService(typeof(IDbContextFactory<AppDbContext>));
        var dbFactory = e as IDbContextFactory<AppDbContext>;
        SeedTeacher(dbFactory);
    }

    private static void SeedTeacher(IDbContextFactory<AppDbContext> dbFactory)
    {
        using var ctx = dbFactory.CreateDbContext();
        
        var teacher = ctx.Teachers.FirstOrDefault(t => t.Id == "teacher-1");
        if (teacher != null) return;
        
        ctx.Teachers.Add(new Teacher
        {
            Id = "teacher-1",
            FirstName = "FirstName",
            LastName = "LastName",
            CreatedAt = default
        });
        ctx.SaveChanges();
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "teacher-1");
        var response = await client.GetAsync("/api/Teacher/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}