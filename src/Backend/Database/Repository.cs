using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Dto;

namespace Backend.Database;

public class Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<Repository> logger)
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
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Attendances
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.ClassId == classId);
    }

    public async Task<AttendancePercentageDto> GetAttendancePercentageAsync(
    Guid studentId,
    DateTimeOffset? from = null,
    DateTimeOffset? to = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var courseCodes = await ctx.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseCode)
            .Distinct()
            .ToListAsync();

        if (courseCodes.Count == 0)
            return new AttendancePercentageDto(studentId, 0, 0, 0, new());

        var classesQuery = ctx.Classes
            .Where(c => courseCodes.Contains(c.CourseCode));

        if (from.HasValue) classesQuery = classesQuery.Where(c => c.StartTime >= from.Value);
        if (to.HasValue) classesQuery = classesQuery.Where(c => c.StartTime <= to.Value);

        var classes = await classesQuery
            .Select(c => new { c.Id, c.CourseCode })
            .ToListAsync();

        var classIds = classes.Select(c => c.Id).ToList();

        var attendedClassIds = await ctx.Attendances
            .Where(a => a.StudentId == studentId && classIds.Contains(a.ClassId))
            .Select(a => a.ClassId)
            .Distinct()
            .ToListAsync();

        var totalClasses = classes.Count;
        var attended = attendedClassIds.Count;
        var overallPct = totalClasses == 0 ? 0 : Math.Round(attended * 100.0 / totalClasses, 1);


        var attendedSet = attendedClassIds.ToHashSet();
        var byCourse = classes
            .GroupBy(c => c.CourseCode)
            .Select(g =>
            {
                var total = g.Count();
                var att = g.Count(x => attendedSet.Contains(x.Id));
                var pct = total == 0 ? 0 : Math.Round(att * 100.0 / total, 1);
                return new CourseAttendanceDto(g.Key, total, att, pct);
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return new AttendancePercentageDto(studentId, totalClasses, attended, overallPct, byCourse);
    }

    public async Task<Class?> GetClassByIdAsync(Guid classId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Classes.FirstOrDefaultAsync(c => c.Id == classId);
    }
}