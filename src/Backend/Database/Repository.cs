using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using SupabaseShared.Models;
using static Supabase.Postgrest.Constants;

namespace Backend.Database;

public class Repository(
    Supabase.Client client,
    ILogger<Repository> logger)
{
    public async Task<bool> IsCameraApiKey(Guid apiKeyId)
    {
        var camKey = await client.From<CameraApiKey>().Where(k => k.ApiKeyId == apiKeyId).Single();
        return camKey != null;
    }
    
    public async Task<List<StudentFace>> GetAllStudentFacesAsync()
    {
        var resp = await client.From<StudentFace>().Get();
        return resp.Models;
    }

    public async Task<ApiKey?> GetApiKeyByHashAsync(string apiKeyHash)
    {
        var resp = await client.From<ApiKey>().Where(k => k.Hash == apiKeyHash && k.IsActive == true).Limit(1).Get();
        return resp.Models.FirstOrDefault();
    }

    public async Task<Attendance> AddAttendanceAsync(Attendance attendance)
    {
        var resp = await client.From<Attendance>().Insert(attendance);
        return resp.Model;
    }

    public async Task<List<StudentFace>> GetStudentFaces()
    {
        var resp = await client.From<StudentFace>().Get();
        return resp.Models;
    }

    public async Task<Student?> GetStudentByFacePath(string path)
    {
        var resp = await client.From<StudentFace>().Where(f => f.FaceImagePath == path).Single();
        return resp?.Student;
    }

    public async Task<bool> IsStudentEnrolledInCourseAsync(Guid studentId, string courseCode, int courseYearId,
        int courseSemesterCode)
    {
        var filters = new List<IPostgrestQueryFilter>
        {
            new QueryFilter<Enrollment, Guid>(x => x.StudentId, Operator.Equals, studentId),
            new QueryFilter<Enrollment, string>(x => x.CourseCode, Operator.Equals, courseCode),
            new QueryFilter<Enrollment, int>(x => x.CourseYear, Operator.Equals, courseYearId),
            new QueryFilter<Enrollment, int>(x => x.CourseSemesterCode, Operator.Equals, courseSemesterCode)
        };
        var resp = await client.From<Enrollment>()
            .And(filters)
            .Single();
        return resp != null;
    }

    public async Task<Attendance> UpsertAttendanceAsync(Guid studentId, Guid classId)
    {
        var attendance = new Attendance
        {
            StudentId = studentId,
            ClassId = classId,
            Timestamp = DateTimeOffset.UtcNow
        };
        var resp = await client.From<Attendance>().Upsert(attendance);
        return resp.Model;
    }

    public async Task<Class?> GetClassByLocationTimeAsync(string location, DateTimeOffset time)
    {
        var filters = new List<IPostgrestQueryFilter>
        {
            new QueryFilter<Class, string>(c => c.Location, Operator.Equals, location),
            new QueryFilter<Class, DateTimeOffset>(c => c.StartTime, Operator.LessThanOrEqual, time),
            new QueryFilter<Class, DateTimeOffset>(c => c.EndTime, Operator.GreaterThanOrEqual, time)
        };
        var resp = await client.From<Class>()
            .And(filters)
            .Single();
        return resp;
    }

    public async Task<Camera?> GetCameraByApiKeyIdAsync(Guid apiKeyId)
    {
        var camKey = await client.From<CameraApiKey>().Where(k => k.ApiKeyId == apiKeyId).Single();
        if (camKey == null) return null;
        var camResp = await client.From<Camera>().Where(c => c.Id == camKey.CameraId).Single();
        return camResp;
    }

    public async Task<Class?> GetClassByIdAsync(Guid classId)
    {
        var resp = await client.From<Class>().Where(c => c.Id == classId).Single();
        return resp;
    }

    public async Task<Student?> GetStudentById(Guid studentId)
    {
        var resp = await client.From<Student>().Where(s => s.Id == studentId).Single();
        return resp;
    }
}