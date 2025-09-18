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
    
    private static readonly Guid AdminId = Guid.NewGuid();
    
    private static readonly Guid CameraId = Guid.NewGuid();
    
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
    
    private static void SeedCamera(AppService appService)
    {
        var camera = new Camera
        {
            Id = CameraId,
            Name = "Test Camera",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        appService.CreateCameraAsync(camera).GetAwaiter().GetResult();
    }
    
    private static void SeedAdmin(AppService appService)
    {
        var admin = new Admin
        {
            Id = AdminId,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "admin@gmail.com",
            Permissions = AdminPermissions.FullAccess
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
    public async Task TestUnauthorizedWhenNoPermissions()
    {
        var appService = _mockAppBuilder.Services.GetRequiredService<AppService>();
        var admin = new Admin
        {
            FirstName = "NoPerms",
            LastName = "Admin",
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "None@none.com",
            Permissions = AdminPermissions.None
        };
        await appService.AddAdminAsync(admin);
        
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", nameof(TestUnauthorizedWhenNoPermissions));
        var response = await client.GetAsync("/api/Admin/teachers");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.GetAsync("/api/Admin/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestAddTeacherWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var newTeacher = new NewTeacherReq
        {
            FirstName = "New",
            LastName = "Teacher"
        };

        var response = await client.PostAsJsonAsync("/api/Admin/teacher", newTeacher);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdTeacher = await response.Content.ReadFromJsonAsync<Teacher>();
        Assert.NotNull(createdTeacher);
        
        Assert.Equal("New", createdTeacher.FirstName);
        Assert.Equal("Teacher", createdTeacher.LastName);
    }

    [Fact]
    public async Task TestGetCourseWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.GetAsync("/api/Admin/courses/CS101/2023/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task TestExportAttendanceWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.GetAsync("/api/Admin/courses/CS101/2023/1/attendance/export");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }
    
    [Fact] 
    public async Task TestExportAttendanceWhenNoCourse()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.GetAsync("/api/Admin/courses/NOPE/2023/1/attendance/export");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task TestGetAttendanceWhenAdmin()
    {
        var appService = _mockAppBuilder.Services.GetRequiredService<AppService>();
        var newStudent = new Student
        {
            FirstName = "Student",
            LastName = "User",
        };
        await appService.AddStudentAsync(newStudent);
        var newClass = new Class
        {
            CourseCode = "CS101",
            CourseYearId = 2023,
            CourseSemesterCode = 1,
            StartTime = DateTimeOffset.UtcNow.AddHours(-1),
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Location = "Room 101"
        };
        await appService.AddClassAsync(newClass);
        var newAttendance = new Attendance
        {
            StudentId = newStudent.Id,
            ClassId = newClass.Id,
            Timestamp = DateTimeOffset.UtcNow,
        };
        await appService.AddAttendanceAsync(newAttendance);
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.GetAsync("/api/Admin/courses/CS101/2023/1/attendance/export");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var csvContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(newStudent.Id.ToString(), csvContent);
    }
    
    [Fact]
    public async Task TestExportAttendanceWhenNoPerms()
    {
        var appService = _mockAppBuilder.Services.GetRequiredService<AppService>();
        var admin = new Admin
        {
            FirstName = "NoPerms",
            LastName = "Admin",
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "none@none.com",
            Permissions = AdminPermissions.FullAccess & ~AdminPermissions.ExportData
        };
        await appService.AddAdminAsync(admin);
        
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nameof(TestExportAttendanceWhenNoPerms));
        var response = await client.GetAsync("/api/Admin/courses/CS101/2023/1/attendance/export");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TestAddCourseSuccessWhenAdmin()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
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
        Assert.Equal("CS102", createdCourse.Code);
        Assert.Equal(2023, createdCourse.Year);
        Assert.Equal(1, createdCourse.SemesterCode);
        Assert.Equal("Data Structures", createdCourse.Name);
    }
    
    [Fact]
    public async Task CreateCameraSucceeds()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());

        var newCamera = new NewCameraReq()
        {
            Name = "New Camera",
            Location = "New Location"
        };
        var response = await client.PostAsJsonAsync("/api/Admin/cameras", newCamera);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdCamera = await response.Content.ReadFromJsonAsync<Camera>();
        Assert.NotNull(createdCamera);
    }
    
    [Fact]
    public async Task GenerateCameraApiKeySucceeds()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        
        var response = await client.PostAsync($"/api/Admin/cameras/{CameraId}/regenerate-key/{0}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GenerateCameraApiKeyFailsWhenNoCamera()
    {
        var client = _mockAppBuilder.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminId.ToString());
        var response = await client.PostAsync($"/api/Admin/cameras/{Guid.NewGuid()}/regenerate-key/{0}", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}