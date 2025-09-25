using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Services;

namespace Backend.Database;

public class Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<Repository> logger, SupabaseRest rest)
{



    public async Task<bool> IsCameraApiKey(Guid apiKeyId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.CameraApiKeys.AnyAsync(k => k.ApiKeyId == apiKeyId);
    }

    public async Task<ApiKey?> GetApiKeyByHashAsync(string apiKeyHash)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.ApiKeys.FirstOrDefaultAsync(k => k.Hash == apiKeyHash && k.IsActive);
    }

    public async Task<Attendance> AddAttendanceAsync(Attendance attendance)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.Attendances.Add(attendance);
        await ctx.SaveChangesAsync();
        return attendance;
    }

    public async Task<Student?> GetStudentByFaceId(string recognizedFacePersonId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsStudentEnrolledInCourseAsync(Guid studentId, string courseCode, int courseYearId,
        int courseSemesterCode)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Enrollments.AnyAsync(e =>
            e.StudentId == studentId &&
            e.CourseCode == courseCode &&
            e.CourseYearId == courseYearId &&
            e.CourseSemesterCode == courseSemesterCode);
    }

    public async Task<Attendance> CreateAttendanceAsync(Guid studentId, Guid classId)
    {
        /*
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var attendance = new Attendance
        {
            StudentId = studentId,
            ClassId = classId,
            Timestamp = DateTimeOffset.UtcNow
        };
        ctx.Attendances.Add(attendance);
        await ctx.SaveChangesAsync();
        return attendance;
        */
        var payload = new
        {
            StudentId = studentId,
            ClassId = classId,
            Timestamp = DateTimeOffset.UtcNow
        };

        var rows = await rest.InsertAsync<Attendance>(
        "Attendance", payload, onConflict: "ClassId,StudentId", ignoreDuplicates: true);

        // If duplicate was ignored, PostgREST returns no rows – fetch the latest existing one
        if (rows is null || rows.Length == 0)
        {
            var existing = await GetAttendanceAsync(studentId, classId);
            if (existing is not null) return existing;
            throw new InvalidOperationException("Insert Attendance failed");
        }

        return rows[0];
    }

    public async Task<Class?> GetClassByLocationTimeAsync(string location, DateTimeOffset time)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Classes.FirstOrDefaultAsync(c => c.Location == location && c.StartTime <= time && c.EndTime >= time);
    }

    public async Task<Camera?> GetCameraByApiKeyIdAsync(Guid apiKeyId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var cameraApiKey = await ctx.CameraApiKeys
            .Include(k => k.Camera)
            .FirstOrDefaultAsync(k => k.ApiKeyId == apiKeyId);
        return cameraApiKey?.Camera;
    }

    public async Task<Attendance?> GetAttendanceAsync(Guid studentId, Guid classId)
    {
        //await using var ctx = await dbContextFactory.CreateDbContextAsync();    //old
        //return await ctx.Attendances
        //    .OrderByDescending(a => a.Timestamp)
        //    .FirstOrDefaultAsync(a => a.StudentId == studentId && a.ClassId == classId);  //old
        var rows = await rest.GetAsync<Attendance>(
            $"Attendance?StudentId=eq.{studentId}&ClassId=eq.{classId}&select=*&order=Timestamp.desc&limit=1");
        return rows?.FirstOrDefault();
    }

    public async Task<Class?> GetClassByIdAsync(Guid classId)
    {
        //await using var ctx = await dbContextFactory.CreateDbContextAsync();   //old
        //return await ctx.Classes.FirstOrDefaultAsync(c => c.Id == classId);    //old
        var rows = await rest.GetAsync<Class>($"Classes?Id=eq.{classId}&select=*");
        return rows?.FirstOrDefault();
    }
}