using Backend.Models;
using Microsoft.EntityFrameworkCore;
using ResultSharp;

namespace Backend.Database;

public class AppService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    AppCache appCache,
    ILogger<AppService> logger)
{
    public async Task<Student?> GetStudentByIdAsync(string studentId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Students.FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Students.ToListAsync();
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Courses.ToListAsync();
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
            .Where(a => a.Class!.CourseCode == courseCode &&
                        a.Class.CourseYearId == courseYear &&
                        a.Class.CourseSemesterCode == courseSemesterCode)
            .ToListAsync();
    }

    #region Teacher

    public async Task<Teacher?> GetTeacherByIdAsync(string teacherId)
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


    public async Task<Result<Teacher, Errors.InsertError>> AddTeacherAsync(Teacher teacher)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var existing = await ctx.Teachers.AnyAsync(t => t.Id == teacher.Id);
        if (existing) return Result.Err<Teacher, Errors.InsertError>(Errors.InsertError.Conflict);

        ctx.Teachers.Add(teacher);
        await ctx.SaveChangesAsync();
        return Result.Ok<Teacher, Errors.InsertError>(teacher);
    }

    public async Task<Result<bool, Errors.DeleteError>> DeleteTeacherAsync(string teacherId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var teacher = await ctx.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher == null) return Result.Err<bool, Errors.DeleteError>(Errors.DeleteError.NotFound);

        ctx.Teachers.Remove(teacher);
        await ctx.SaveChangesAsync();
        await appCache.EvictTeacherAsync(teacherId);
        return Result.Ok<bool, Errors.DeleteError>(true);
    }

    public async Task<bool> TeacherExistsAsync(string teacherId)
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

    public async Task<Admin?> GetAdminByIdAsync(string adminId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Admins.FirstOrDefaultAsync(a => a.Id == adminId);
    }

    public async Task<bool> AdminExistsAsync(string adminId)
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
        return Result.Ok<Admin, Errors.InsertError>(admin);
    }

    #endregion
}