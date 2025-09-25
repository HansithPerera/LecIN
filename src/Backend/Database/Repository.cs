using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Dto;
using Backend.Services;

namespace Backend.Database;

public class Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<Repository> logger, SupabaseRest rest)
{
    private readonly SupabaseRest _rest = rest;
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


    //
    public async Task<Enrollment[]> GetEnrollmentsForStudentAsync(Guid studentId)
    {
        var rows = await rest.GetAsync<Enrollment>(
            $"Enrollments?StudentId=eq.{studentId}&select=CourseCode,CourseYearId,CourseSemesterCode");
        return rows ?? Array.Empty<Enrollment>();
    }

    public async Task<Class[]> GetClassesForEnrollmentsAsync(Enrollment[] enrollments, DateTimeOffset asOf)
    {
        if (enrollments.Length == 0) return Array.Empty<Class>();

        var all = new List<Class>();
        foreach (var e in enrollments)
        {
            // StartTime <= asOf : only classes that should have happened by now
            var url =
                $"Classes?CourseCode=eq.{e.CourseCode}" +
                $"&CourseYear=eq.{e.CourseYearId}" +
                $"&CourseSemesterCode=eq.{e.CourseSemesterCode}" +
                $"&StartTime=lte.{Uri.EscapeDataString(asOf.ToString("o"))}" +
                $"&select=Id,CourseCode,StartTime,EndTime";
            var rows = await rest.GetAsync<Class>(url);
            if (rows != null) all.AddRange(rows);
        }
        // distinct by Id (in case of overlap)
        return all.GroupBy(c => c.Id).Select(g => g.First()).ToArray();
    }

    public async Task<Attendance[]> GetAttendanceForStudentInClassesAsync(
        Guid studentId, IEnumerable<Guid> classIds)
    {
        var ids = classIds.Distinct().ToArray();
        if (ids.Length == 0) return Array.Empty<Attendance>();

        var acc = new List<Attendance>();
        // chunk the IN() list so the URL doesn't get too long
        for (int i = 0; i < ids.Length; i += 50)
        {
            var chunk = ids.Skip(i).Take(50).Select(x => x.ToString());
            var inList = string.Join(",", chunk);
            var url =
                $"Attendance?StudentId=eq.{studentId}" +
                $"&ClassId=in.({inList})" +
                $"&select=ClassId,StudentId,Timestamp";
            var rows = await rest.GetAsync<Attendance>(url);
            if (rows != null) acc.AddRange(rows);
        }
        return acc.ToArray();
    }

    public async Task<AttendancePercentResponse> GetAttendancePercentageAsync(
        Guid studentId, DateTimeOffset? asOf = null)
    {
        var at = asOf ?? DateTimeOffset.UtcNow;

        var enrollments = await GetEnrollmentsForStudentAsync(studentId);
        var classes = await GetClassesForEnrollmentsAsync(enrollments, at);
        var classIds = classes.Select(c => c.Id).ToArray();

        var attendance = await GetAttendanceForStudentInClassesAsync(studentId, classIds);

        var total = classIds.Length;
        var attended = attendance.Select(a => a.ClassId).Distinct().Count();
        var percent = total == 0 ? 0 : Math.Round((double)attended / total * 100, 1);

        return new AttendancePercentResponse(studentId, attended, total, percent, at);
    }
    //
}