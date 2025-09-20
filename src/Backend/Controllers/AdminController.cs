using System.Security.Claims;
using Backend.Database;
using Backend.Dto.Req;
using Backend.Dto.Resp;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.DateTime;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.AdminAuthorizationPolicy)]
public class AdminController(Repository repo, CourseService courseService, CameraService cameraService, TeacherService teacherService) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(typeof(Admin), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var guid))
            return NotFound("User ID not found in token.");
        var admin = await repo.GetAdminByIdAsync(guid);
        return admin == null
            ? NotFound("Admin not found.")
            : Ok(admin);
    }

    [HttpGet("teachers")]
    [ProducesResponseType(typeof(List<Teacher>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadTeachersPermission)]
    public async Task<IActionResult> GetAllTeachers()
    {
        return Ok(await repo.GetAllTeachersAsync());
    }

    [HttpGet("courses")]
    [EndpointDescription("Retrieve a list of all courses.")]
    [EndpointName("GetAllCourses")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadCoursesPermission)]
    public async Task<IActionResult> GetAllCourses()
    {
        return Ok(await repo.GetAllCoursesAsync());
    }

    [HttpGet("cameras")]
    [ProducesResponseType(typeof(List<Camera>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadCamerasPermission)]
    public async Task<IActionResult> GetAllCameras()
    {
        return Ok(await repo.GetAllCamerasAsync());
    }

    [HttpGet("cameras/{id}")]
    [ProducesResponseType(typeof(Camera), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadCamerasPermission)]
    public async Task<IActionResult> GetCameraById(Guid id)
    {
        var camera = await repo.GetCameraByIdAsync(id);
        return camera == null
            ? NotFound("Camera not found.")
            : Ok(camera);
    }
    
    [HttpPost("cameras")]
    [ProducesResponseType(typeof(Camera), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Constants.AdminManageCamerasPermission)]
    public async Task<IActionResult> CreateCamera([FromBody] NewCameraReq req)
    {
        var creationResult = await cameraService.CreateCameraAsync(req.Name, req.Location);
        if (creationResult.IsErr)
            return creationResult.UnwrapErr() switch
            {
                Errors.NewCameraError.MissingLocation => BadRequest("Camera location is required."),
                Errors.NewCameraError.MissingName => BadRequest("Camera name is required."),
                Errors.NewCameraError.Conflict => Conflict("A camera at the same location already exists."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unknown error occurred.")
            };
        var createdCamera = creationResult.Unwrap();
        return CreatedAtAction(nameof(GetCameraById), new { id = createdCamera.Id }, createdCamera);
    }

    [HttpPost("cameras/{cameraId:guid}/regenerate-key/{role:int}")]
    [ProducesResponseType(typeof(Camera), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminManageCamerasPermission)]
    public async Task<IActionResult> RegenerateCameraApiKey(Guid cameraId, int role)
    {
        if (!Enum.IsDefined(typeof(ApiKeyRole), role))
            return BadRequest("Invalid role specified. Must be 1 (Primary) or 2 (Secondary).");

        var generateResult = await cameraService.RegenerateCameraApiKeyAsync(cameraId, (ApiKeyRole) role);
        if (generateResult.IsErr)
            return generateResult.UnwrapErr() switch
            {
                Errors.GenerateCameraApiKeyError.CameraNotFound => NotFound("Camera not found."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unknown error occurred.")
            };
        
        var generatedKey = generateResult.Unwrap();
        return Ok(new GeneratedApiKeyDto
        {
            ApiKeyId = generatedKey.CameraApiKey.ApiKeyId,
            ApiKey = generatedKey.UnhashedKey,
            Prefix = generatedKey.CameraApiKey.ApiKey!.Prefix
        });
    }

    [HttpGet("students")]
    [ProducesResponseType(typeof(List<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadStudentsPermission)]
    public async Task<IActionResult> GetAllStudents()
    {
        return Ok(await repo.GetAllStudentsAsync());
    }

    [HttpGet("courses/{courseCode}/{courseYear}/{courseSemesterCode}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminReadCoursesPermission)]
    public async Task<IActionResult> GetCourse(string courseCode, int courseYear, int courseSemesterCode)
    {
        var course = await repo.GetCourseAsync(courseCode, courseYear, courseSemesterCode);
        return course == null
            ? NotFound("Course not found.")
            : Ok(course);
    }


    [HttpPost("course")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Constants.AdminManageCoursesPermission)]
    public async Task<IActionResult> CreateCourse([FromBody] NewCourseReq course)
    {
        var creationResult = await courseService.AddNewCourse(course);
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
        return CreatedAtAction(nameof(GetAllCourses),
            new { code = createdCourse.Code, year = createdCourse.Year, semesterCode = createdCourse.SemesterCode },
            createdCourse);
    }

    [HttpPost("teacher")]
    [ProducesResponseType(typeof(Teacher), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Constants.AdminManageTeachersPermission)]
    public async Task<IActionResult> CreateTeacher([FromBody] NewTeacherReq teacher)
    {
        var creationResult = await teacherService.AddNewTeacherAsync(teacher);
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

    [HttpGet("courses/{courseCode}/{courseYear}/{courseSemesterCode}/attendance/export")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Constants.AdminExportDataPermission)]
    [Authorize(Policy = Constants.AdminReadCoursesPermission)]
    public async Task<IActionResult> ExportCourseAttendanceCsv(string courseCode, int courseYear,
        int courseSemesterCode)
    {
        var result = await courseService.ExportCourseAttendanceToCsv(courseCode, courseYear, courseSemesterCode);
        if (result.IsErr)
            return result.UnwrapErr() switch
            {
                Errors.GetError.NotFound => NotFound("Course not found."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unknown error occurred.")
            };

        var resultStream = result.Unwrap();
        var timestamp = UtcNow.ToString("yyyyMMdd_HHmmss");
        return File(resultStream, "text/csv",
            $"{courseCode}_{courseYear}_S{courseSemesterCode}_attendance_{timestamp}.csv");
    }
}