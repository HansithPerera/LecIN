using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.TeacherAuthorizationPolicy)]
public class TeacherController(IDbContextFactory<AppDbContext> ctxFactory) : ControllerBase
{
    #region GET

    [HttpGet("profile")]
    [ProducesResponseType(typeof(Teacher), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.Identity?.Name;
        await using var ctx = await ctxFactory.CreateDbContextAsync();
        var teacher = await ctx.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
        if (teacher == null) return NotFound();
        return Ok(teacher);
    }

    [HttpGet("courses")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCourses()
    {
        await using var ctx = await ctxFactory.CreateDbContextAsync();
        var courses = await ctx.Courses.ToListAsync();
        return Ok(courses);
    }

    [HttpGet("course/{courseId}/students")]
    [ProducesResponseType(typeof(List<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentsInCourse(int courseId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("course/{courseId}/classes")]
    [ProducesResponseType(typeof(List<Class>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassesInCourse(int courseId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("course/{courseId}/classes/{classId}/attendance")]
    [ProducesResponseType(typeof(List<Attendance>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttendanceInClass(int courseId, int classId)
    {
        throw new NotImplementedException();
    }

    #endregion
}