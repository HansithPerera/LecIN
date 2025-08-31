using Backend.Database;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.StudentAuthorizationPolicy)]
public class StudentController(AppService repository) : ControllerBase
{
    #region Post

    [HttpPost("checkin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CheckInToCourse([FromBody] string courseCode)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Get

    [HttpGet("profile")]
    [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        throw new NotImplementedException();
    }

    [HttpGet("classes")]
    [ProducesResponseType(typeof(List<Class>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetClasses()
    {
        throw new NotImplementedException();
    }

    [HttpGet("courses")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCourses()
    {
        throw new NotImplementedException();
    }

    [HttpGet("courses/{courseCode}/teachers")]
    [ProducesResponseType(typeof(List<Teacher>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTeachersForCourse(string courseCode)
    {
        throw new NotImplementedException();
    }

    #endregion
}