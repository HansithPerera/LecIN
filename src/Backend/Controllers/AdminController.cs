using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.AdminAuthorizationPolicy)]
public class AdminController(IDbContextFactory<DataContext> ctxFactory) : ControllerBase
{
    [HttpGet("teachers")]
    [ProducesResponseType(typeof(List<Teacher>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllTeachers()
    {
        var userId = User.Identity?.Name;
        await using var ctx = await ctxFactory.CreateDbContextAsync();
        var teachers = await ctx.Teachers.ToListAsync();
        return Ok(teachers);
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

    [HttpGet("students")]
    [ProducesResponseType(typeof(List<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllStudents()
    {
        await using var ctx = await ctxFactory.CreateDbContextAsync();
        var students = await ctx.Students.ToListAsync();
        return Ok(students);
    }
}