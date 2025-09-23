using Backend.Database;
using Backend.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher/attendance")]
public class TeacherAttendanceController(Repository repo) : ControllerBase
{
    // Keep it simple: any signed-in user for now (swap to your Teacher policy later)
    [Authorize] // TODO: [Authorize(Policy = "<YourTeacherPolicyName>")]
    [HttpGet("percentage/{studentId:guid}")]
    public async Task<ActionResult<AttendancePercentageDto>> GetStudentAttendancePercent(
        Guid studentId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var dto = await repo.GetAttendancePercentageAsync(studentId, from, to);
        return Ok(dto);
    }
}

