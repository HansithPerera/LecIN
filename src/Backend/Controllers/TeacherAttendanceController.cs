using Backend.Database;
using Backend.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher/attendance")]
[AllowAnonymous]
public class TeacherAttendanceController(Repository repo) : ControllerBase
{
    
    //[Authorize] 
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

