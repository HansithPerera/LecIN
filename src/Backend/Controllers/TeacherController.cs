using Backend.Database;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.TeacherAuthorizationPolicy)]
public class TeacherController(AppService repository) : ControllerBase
{
    #region GET

    [HttpGet("profile")]
    [ProducesResponseType(typeof(Teacher), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return NotFound("User ID not found in token.");
        
        if (!Guid.TryParse(userId, out var userGuid))
            return BadRequest("Invalid User ID format.");

        var teacher = await repository.GetTeacherByIdAsync(userGuid);
        return teacher == null
            ? NotFound("Teacher not found.")
            : Ok(teacher);
    }

    [HttpGet("courses")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await repository.GetAllCoursesAsync();
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