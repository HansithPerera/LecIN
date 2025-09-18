using Backend.Models;
using Microsoft.EntityFrameworkCore;
using ResultSharp;

namespace Backend.Database;

public class AppService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    AppCache appCache,
    ILogger<AppService> logger)
{
    public async Task<Student?> GetStudentByIdAsync(Guid studentId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Students.FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Students.ToListAsync();
    }
    
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

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Courses.ToListAsync();
    }
    
    public async Task<Student> AddStudentAsync(Student student)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.Students.Add(student);
        await ctx.SaveChangesAsync();
        return student;
    }

    public async Task<List<Student>> GetStudentsInCourseAsync(int courseId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Students
            .Where(s => s.Enrollments!.Any(e =>
                e.CourseSemesterCode == courseId && e.CourseCode == e.CourseCode && e.CourseYearId == e.CourseYearId))
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetCourseAttendance(string courseCode, int courseYear, int courseSemesterCode)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Attendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Include(a => a.Class!.Course)
            .Where(a => a.Class!.CourseCode == courseCode &&
                        a.Class.CourseYearId == courseYear &&
                        a.Class.CourseSemesterCode == courseSemesterCode)
            .ToListAsync();
    }

    #region Teacher

    public async Task<Teacher?> GetTeacherByIdAsync(Guid teacherId)
    {
        var fromCache = await appCache.GetTeacherAsync(teacherId);
        if (fromCache != null) return fromCache;

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var teacher = await ctx.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher != null) await appCache.SetTeacherAsync(teacher);
        return teacher;
    }

    public async Task<List<Teacher>> GetAllTeachersAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Teachers.ToListAsync();
    }

    public async Task<Result<Teacher, Errors.UpdateError>> UpdateTeacherAsync(Teacher teacher)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Teachers.AnyAsync(t => t.Id == teacher.Id);
        if (!existing) return Result.Err<Teacher, Errors.UpdateError>(Errors.UpdateError.NotFound);

        ctx.Teachers.Update(teacher);
        await ctx.SaveChangesAsync();
        await appCache.EvictTeacherAsync(teacher.Id);
        return Result.Ok<Teacher, Errors.UpdateError>(teacher);
    }
    
    public async Task<Result<Class, Errors.InsertError>> AddClassAsync(Class @class)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Classes.AnyAsync(c => c.Id == @class.Id);
        if (existing) return Result.Err<Class, Errors.InsertError>(Errors.InsertError.Conflict);

        ctx.Classes.Add(@class);
        await ctx.SaveChangesAsync();
        return Result.Ok<Class, Errors.InsertError>(@class);
    }

    public async Task<Result<Teacher, Errors.InsertError>> AddTeacherAsync(Teacher teacher)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Teachers.AnyAsync(t => t.Id == teacher.Id);
        if (existing) return Result.Err<Teacher, Errors.InsertError>(Errors.InsertError.Conflict);

        ctx.Teachers.Add(teacher);
        await ctx.SaveChangesAsync();
        return Result.Ok<Teacher, Errors.InsertError>(teacher);
    }

    public async Task<Result<bool, Errors.DeleteError>> DeleteTeacherAsync(Guid teacherId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var teacher = await ctx.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher == null) return Result.Err<bool, Errors.DeleteError>(Errors.DeleteError.NotFound);

        ctx.Teachers.Remove(teacher);
        await ctx.SaveChangesAsync();
        await appCache.EvictTeacherAsync(teacherId);
        return Result.Ok<bool, Errors.DeleteError>(true);
    }

    public async Task<bool> TeacherExistsAsync(Guid teacherId)
    {
        var teacher = await GetTeacherByIdAsync(teacherId);
        return teacher != null;
    }

    #endregion

    #region admin

    public async Task<Result<Course, Errors.InsertError>> AddCourseAsync(Course course)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Courses.AnyAsync(c =>
            c.Code == course.Code && c.Year == course.Year && c.SemesterCode == course.SemesterCode);
        if (existing) return Result.Err<Course, Errors.InsertError>(Errors.InsertError.Conflict);

        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        return Result.Ok<Course, Errors.InsertError>(course);
    }

    public async Task<Course?> GetCourseAsync(string courseCode, int courseYear, int courseSemesterCode)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Courses.FirstOrDefaultAsync(c =>
            c.Code == courseCode && c.Year == courseYear && c.SemesterCode == courseSemesterCode);
    }

    public async Task<Admin?> GetAdminByIdAsync(Guid adminId)
    {
        var fromCache = await appCache.GetAdminAsync(adminId);
        if (fromCache != null) return fromCache;
        
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Admins.FirstOrDefaultAsync(a => a.Id == adminId);
    }

    public async Task<bool> AdminExistsAsync(Guid adminId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Admins.AnyAsync(a => a.Id == adminId);
    }

    public async Task<Result<Admin, Errors.InsertError>> AddAdminAsync(Admin admin)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Admins.AnyAsync(a => a.Id == admin.Id);
        if (existing) return Result.Err<Admin, Errors.InsertError>(Errors.InsertError.Conflict);

        ctx.Admins.Add(admin);
        await ctx.SaveChangesAsync();
        await appCache.SetAdminAsync(admin);
        return Result.Ok<Admin, Errors.InsertError>(admin);
    }

    #endregion
}