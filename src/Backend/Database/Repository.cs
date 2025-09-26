using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Dto;
using Backend.Services;

namespace Backend.Database;

public class Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<Repository> logger,
    SupabaseRest rest)
{
    private readonly SupabaseRest _rest = rest;

    // REST-only DTO matching Supabase "Enrollments" columns exactly
    private sealed record EnrollmentRow(
        Guid StudentId,
        string CourseCode,
        int CourseYear,
        int CourseSemesterCode
    );

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

    public async Task<bool> IsStudentEnrolledInCourseAsync(Guid studentId, string courseCode, int courseYearId, int courseSemesterCode)
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

    public async Task<Class?> GetClassByIdAsync(Guid classId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Classes.FirstOrDefaultAsync(c => c.Id == classId);
    }

    // ========= Teacher "attendance % (with optional from/to)" using Supabase REST =========
    public async Task<AttendancePercentageDto> GetAttendancePercentageAsync(
        Guid studentId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        // 1) Enrollments for the student (exact column names)
        var enrollments = await _rest.GetAsync<EnrollmentRow>(
            $"Enrollments?StudentId=eq.{studentId}&select=StudentId,CourseCode,CourseYear,CourseSemesterCode");

        if (enrollments is null || enrollments.Length == 0)
            return new AttendancePercentageDto(studentId, 0, 0, 0, new());

        // 2) Classes matching those enrollments (optional StartTime filters)
        var classes = new List<Class>();
        foreach (var e in enrollments)
        {
            var filters = new List<string>
            {
                $"CourseCode=eq.{e.CourseCode}",
                $"CourseYear=eq.{e.CourseYear}",
                $"CourseSemesterCode=eq.{e.CourseSemesterCode}"
            };
            if (from.HasValue) filters.Add($"StartTime=gte.{Uri.EscapeDataString(from.Value.ToString("o"))}");
            if (to.HasValue) filters.Add($"StartTime=lte.{Uri.EscapeDataString(to.Value.ToString("o"))}");

            var url = "Classes?" + string.Join("&", filters) + "&select=Id,CourseCode,StartTime,EndTime";
            var rows = await _rest.GetAsync<Class>(url);
            if (rows != null) classes.AddRange(rows);
        }

        if (classes.Count == 0)
            return new AttendancePercentageDto(studentId, 0, 0, 0, new());

        var classIds = classes.Select(c => c.Id).Distinct().ToArray();

        // 3) Attendance for this student across those classes (chunked IN())
        var attendedSet = new HashSet<Guid>();
        for (int i = 0; i < classIds.Length; i += 50)
        {
            var chunk = classIds.Skip(i).Take(50);
            var inList = string.Join(",", chunk);
            var url = $"Attendance?StudentId=eq.{studentId}&ClassId=in.({inList})&select=ClassId";
            var rows = await _rest.GetAsync<Attendance>(url);
            if (rows != null)
                foreach (var a in rows)
                    attendedSet.Add(a.ClassId);
        }

        // 4) Compute overall and per-course
        var total = classIds.Length;
        var attended = attendedSet.Count;
        var overallPct = total == 0 ? 0 : Math.Round(attended * 100.0 / total, 1);

        var byCourse = classes
            .GroupBy(c => c.CourseCode)
            .Select(g =>
            {
                var totalC = g.Count();
                var attC = g.Count(x => attendedSet.Contains(x.Id));
                var pctC = totalC == 0 ? 0 : Math.Round(attC * 100.0 / totalC, 1);
                return new CourseAttendanceDto(g.Key, totalC, attC, pctC);
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return new AttendancePercentageDto(studentId, total, attended, overallPct, byCourse);
    }

    // ========= Simple overall % "as of now" (same REST path, internal helpers) =========
    public async Task<AttendancePercentResponse> GetAttendancePercentageAsync(
        Guid studentId, DateTimeOffset? asOf = null)
    {
        var at = asOf ?? DateTimeOffset.UtcNow;

        var enrollments = await GetEnrollmentsForStudentRest(studentId);
        var classes = await GetClassesForEnrollmentsRest(enrollments, at);
        var classIds = classes.Select(c => c.Id).ToArray();

        var attendance = await GetAttendanceForStudentInClassesRest(studentId, classIds);

        var total = classIds.Length;
        var attended = attendance.Select(a => a.ClassId).Distinct().Count();
        var percent = total == 0 ? 0 : Math.Round((double)attended / total * 100, 1);

        return new AttendancePercentResponse(studentId, attended, total, percent, at);
    }

    // ========================== PRIVATE REST HELPERS ===========================

    private async Task<EnrollmentRow[]> GetEnrollmentsForStudentRest(Guid studentId)
    {
        var rows = await _rest.GetAsync<EnrollmentRow>(
            $"Enrollments?StudentId=eq.{studentId}&select=StudentId,CourseCode,CourseYear,CourseSemesterCode");
        return rows ?? Array.Empty<EnrollmentRow>();
    }

    private async Task<Class[]> GetClassesForEnrollmentsRest(EnrollmentRow[] enrollments, DateTimeOffset asOf)
    {
        if (enrollments.Length == 0) return Array.Empty<Class>();

        var all = new List<Class>();
        foreach (var e in enrollments)
        {
            var url =
                $"Classes?CourseCode=eq.{e.CourseCode}" +
                $"&CourseYear=eq.{e.CourseYear}" +
                $"&CourseSemesterCode=eq.{e.CourseSemesterCode}" +
                $"&StartTime=lte.{Uri.EscapeDataString(asOf.ToString("o"))}" +
                $"&select=Id,CourseCode,StartTime,EndTime";
            var rows = await _rest.GetAsync<Class>(url);
            if (rows != null) all.AddRange(rows);
        }
        // distinct by Id
        return all.GroupBy(c => c.Id).Select(g => g.First()).ToArray();
    }

    private async Task<Attendance[]> GetAttendanceForStudentInClassesRest(
        Guid studentId, IEnumerable<Guid> classIds)
    {
        var ids = classIds.Distinct().ToArray();
        if (ids.Length == 0) return Array.Empty<Attendance>();

        var acc = new List<Attendance>();
        for (int i = 0; i < ids.Length; i += 50)
        {
            var chunk = ids.Skip(i).Take(50).Select(x => x.ToString());
            var inList = string.Join(",", chunk);
            var url =
                $"Attendance?StudentId=eq.{studentId}" +
                $"&ClassId=in.({inList})" +
                $"&select=ClassId,StudentId,Timestamp";
            var rows = await _rest.GetAsync<Attendance>(url);
            if (rows != null) acc.AddRange(rows);
        }
        return acc.ToArray();
    }
}












/*
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Dto;
using Backend.Services;

namespace Backend.Database;

public class Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<Repository> logger, SupabaseRest rest)
{
    private sealed record EnrollmentRow(
    Guid StudentId,
    string CourseCode,
    int CourseYear,
    int CourseSemesterCode
    );

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
        // 1) Enrollments for the student (use the REST DTO and exact column names)
        var enrollments = await _rest.GetAsync<EnrollmentRow>(
            $"Enrollments?StudentId=eq.{studentId}&select=StudentId,CourseCode,CourseYear,CourseSemesterCode");

        if (enrollments is null || enrollments.Length == 0)
            return new AttendancePercentageDto(studentId, 0, 0, 0, new());

        // 2) Classes matching those enrollments (optionally filter by from/to on StartTime)
        var classes = new List<Class>();
        foreach (var e in enrollments)
        {
            var filters = new List<string>
    {
        $"CourseCode=eq.{e.CourseCode}",
        $"CourseYear=eq.{e.CourseYear}",
        $"CourseSemesterCode=eq.{e.CourseSemesterCode}"
    };
            if (from.HasValue) filters.Add($"StartTime=gte.{Uri.EscapeDataString(from.Value.ToString("o"))}");
            if (to.HasValue) filters.Add($"StartTime=lte.{Uri.EscapeDataString(to.Value.ToString("o"))}");

            var url = "Classes?" + string.Join("&", filters) + "&select=Id,CourseCode,StartTime,EndTime";
            var rows = await _rest.GetAsync<Class>(url);
            if (rows != null) classes.AddRange(rows);
        }

        if (classes.Count == 0)
            return new AttendancePercentageDto(studentId, 0, 0, 0, new());

        var classIds = classes.Select(c => c.Id).Distinct().ToArray();

        // 3) Attendance for this student across those classes (chunk IN() to keep URL short)
        var attendedSet = new HashSet<Guid>();
        for (int i = 0; i < classIds.Length; i += 50)
        {
            var chunk = classIds.Skip(i).Take(50);
            var inList = string.Join(",", chunk);
            var url = $"Attendance?StudentId=eq.{studentId}&ClassId=in.({inList})&select=ClassId,StudentId,Timestamp";
            var rows = await _rest.GetAsync<Attendance>(url);
            if (rows != null)
                foreach (var a in rows)
                    attendedSet.Add(a.ClassId);
        }

        // 4) Compute overall and per-course
        var total = classIds.Length;
        var attended = attendedSet.Count;
        var overallPct = total == 0 ? 0 : Math.Round(attended * 100.0 / total, 1);

        var byCourse = classes
            .GroupBy(c => c.CourseCode)
            .Select(g =>
            {
                var totalC = g.Count();
                var attC = g.Count(x => attendedSet.Contains(x.Id));
                var pctC = totalC == 0 ? 0 : Math.Round(attC * 100.0 / totalC, 1);
                return new CourseAttendanceDto(g.Key, totalC, attC, pctC);
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return new AttendancePercentageDto(studentId, total, attended, overallPct, byCourse);
    }

    /*
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
    */



/*
    public async Task<Class?> GetClassByIdAsync(Guid classId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Classes.FirstOrDefaultAsync(c => c.Id == classId);
    }
    


    //
    public async Task<Enrollment[]> GetEnrollmentsForStudentAsync(Guid studentId)
    {
        var rows = await _rest.GetAsync<EnrollmentRow>(
            $"Enrollments?StudentId=eq.{studentId}&select=StudentId,CourseCode,CourseYear,CourseSemesterCode");
        return rows ?? Array.Empty<EnrollmentRow>();
    }

    public async Task<Class[]> GetClassesForEnrollmentsAsync(EnrollmentRow[] enrollments, DateTimeOffset asOf)
    {
        if (enrollments.Length == 0) return Array.Empty<Class>();

        var all = new List<Class>();
        foreach (var e in enrollments)
        {
            var url =
                $"Classes?CourseCode=eq.{e.CourseCode}" +
                $"&CourseYear=eq.{e.CourseYear}" +
                $"&CourseSemesterCode=eq.{e.CourseSemesterCode}" +
                $"&StartTime=lte.{Uri.EscapeDataString(asOf.ToString("o"))}" +
                $"&select=Id,CourseCode,StartTime,EndTime";
            var rows = await _rest.GetAsync<Class>(url);
            if (rows != null) all.AddRange(rows);
        }
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
            var rows = await _rest.GetAsync<Attendance>(url);
            if (rows != null) acc.AddRange(rows);
        }
        return acc.ToArray();
    }

    public async Task<AttendancePercentResponse> GetAttendancePercentageAsync(
    Guid studentId, DateTimeOffset? asOf = null)
    {
        var at = asOf ?? DateTimeOffset.UtcNow;

        var enrollments = await _rest.GetAsync<EnrollmentRow>(
            $"Enrollments?StudentId=eq.{studentId}&select=StudentId,CourseCode,CourseYear,CourseSemesterCode");

        var classes = await GetClassesForEnrollmentsAsync(enrollments ?? Array.Empty<EnrollmentRow>(), at);
        var classIds = classes.Select(c => c.Id).ToArray();

        var attendance = await GetAttendanceForStudentInClassesAsync(studentId, classIds);

        var total = classIds.Length;
        var attended = attendance.Select(a => a.ClassId).Distinct().Count();
        var percent = total == 0 ? 0 : Math.Round((double)attended / total * 100, 1);

        return new AttendancePercentResponse(studentId, attended, total, percent, at);
    }
    //
}
*/