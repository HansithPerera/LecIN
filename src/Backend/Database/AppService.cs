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
        throw new NotImplementedException();
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

    #endregion
}