using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Database;
using Backend.Dto.Req;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class AdminIntegrationTests: IClassFixture<MockAppBuilder>
{
    private readonly MockAppBuilder _mockAppBuilder;

    public AdminIntegrationTests(MockAppBuilder mockAppBuilder)
    {
        _mockAppBuilder = mockAppBuilder;
        var dataService = _mockAppBuilder.Services.GetRequiredService<AppService>();
        SeedAdmin(dataService);
        SeedCourse(dataService);
    }
    
    private void SeedCourse(AppService appService)
    {
        var course = new Course
        {
            Code = "CS101",
            Year = 2023,
            SemesterCode = 1,
            Name = "Introduction to Computer Science"
        };
        
        appService.AddCourseAsync(course).GetAwaiter().GetResult();
    }
    
    private static void SeedAdmin(AppService appService)
    {
        var admin = new Admin
        {
            Id = "admin-1",
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "admin@gmail.com"
        };
        
        appService.AddAdminAsync(admin).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task TestUnauthorizedWhenNoCredentials()
    {
        var client = _mockAppBuilder.CreateClient();
        var response = await client.GetAsync("/api/Admin/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestForbiddenWhenNotAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "teacher-1");
        var response = await client.GetAsync("/api/Admin/profile");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TestSuccessWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-1");
        var response = await client.GetAsync("/api/Admin/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestAddTeacherWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-1");
        var newTeacher = new NewTeacherReq
        {
            FirstName = "New",
            LastName = "Teacher"
        };

        var response = await client.PostAsJsonAsync("/api/Admin/teacher", newTeacher);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdTeacher = await response.Content.ReadFromJsonAsync<Teacher>();
        Assert.NotNull(createdTeacher);
        
        Assert.Equal("New", createdTeacher!.FirstName);
        Assert.Equal("Teacher", createdTeacher.LastName);
    }

    [Fact]
    public async Task TestGetCourseWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-1");
        var response = await client.GetAsync("/api/Admin/courses/CS101/2023/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestAddCourseSuccessWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-1");
        var newCourseReq = new NewCourseReq
        {
            Code = "CS102",
            Year = 2023,
            SemesterCode = 1,
            Name = "Data Structures"
        };
        var response = await client.PostAsJsonAsync("/api/Admin/course", newCourseReq);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdCourse = await response.Content.ReadFromJsonAsync<Course>();
        Assert.NotNull(createdCourse);
        Assert.Equal("CS102", createdCourse!.Code);
        Assert.Equal(2023, createdCourse.Year);
        Assert.Equal(1, createdCourse.SemesterCode);
        Assert.Equal("Data Structures", createdCourse.Name);
    }
}