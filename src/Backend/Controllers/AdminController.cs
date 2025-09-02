using Backend.Database;
using Backend.Dto.Req;
using Backend.Models;
using Backend.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.AdminAuthorizationPolicy)]
public class AdminController(AppService repository) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(typeof(Admin), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return NotFound("User ID not found in token.");
        var admin = await repository.GetAdminByIdAsync(userId);
        return admin == null
            ? NotFound("Admin not found.")
            : Ok(admin);
    }

    [HttpGet("teachers")]
    [ProducesResponseType(typeof(List<Teacher>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllTeachers()
    {
        return Ok(await repository.GetAllTeachersAsync());
    }

    [HttpGet("courses")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCourses()
    {
        return Ok(await repository.GetAllCoursesAsync());
    }

    [HttpGet("students")]
    [ProducesResponseType(typeof(List<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllStudents()
    {
        return Ok(await repository.GetAllStudentsAsync());
    }
    
    [HttpGet("courses/{courseCode}/{courseYear}/{courseSemesterCode}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourse(string courseCode, int courseYear, int courseSemesterCode)
    {
        var course = await repository.GetCourseAsync(courseCode, courseYear, courseSemesterCode);
        return course == null
            ? NotFound("Course not found.")
            : Ok(course);
    }
    

    [HttpPost("course")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] NewCourseReq course)
    {
        var creationResult = await CourseRules.AddNewCourse(course, repository);
        if (creationResult.IsErr)
            return creationResult.UnwrapErr() switch
            {
                Errors.NewCourseError.MissingCode => BadRequest("Course code is required."),
                Errors.NewCourseError.MissingName => BadRequest("Course name is required."),
                Errors.NewCourseError.InvalidYear => BadRequest("Year must be between 1 and 5."),
                Errors.NewCourseError.InvalidSemesterCode => BadRequest("Semester code must be between 1 and 3."),
                Errors.NewCourseError.Conflict => Conflict("A course with the same code already exists."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unknown error occurred.")
            };
        var createdCourse = creationResult.Unwrap();
        return CreatedAtAction(nameof(GetAllCourses), new { code = createdCourse.Code, year = createdCourse.Year, semesterCode = createdCourse.SemesterCode }, createdCourse);
    }

    [HttpPost("teacher")]
    [ProducesResponseType(typeof(Teacher), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeacher([FromBody] NewTeacherReq teacher)
    {
        var creationResult = await TeacherRules.AddNewTeacherAsync(teacher, repository);
        if (creationResult.IsErr)
            return creationResult.UnwrapErr() switch
            {
                Errors.NewTeacherError.MissingFirstName => BadRequest("First name is required."),
                Errors.NewTeacherError.MissingLastName => BadRequest("Last name is required."),
                Errors.NewTeacherError.Conflict => Conflict("A teacher with the same ID already exists."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unknown error occurred.")
            };
        var createdTeacher = creationResult.Unwrap();
        return CreatedAtAction(nameof(GetAllTeachers), new { id = createdTeacher.Id }, createdTeacher);
    }
}