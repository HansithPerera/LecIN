using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Constants.StudentAuthorizationPolicy)]
public class StudentController(AppService service) : ControllerBase
{
    // GET: api/student/attendance
    [HttpGet("attendance")]
    [ProducesResponseType(typeof(List<AttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceHistory()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User not found");

        var history = await service.GetStudentAttendanceHistoryAsync(userId);
        return Ok(history);
    }

    // GET: api/student/attendance/{classId}
    [HttpGet("attendance/{classId}")]
    [ProducesResponseType(typeof(AttendanceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttendanceDetails(int classId)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User not found");

        var attendance = await service.GetAttendanceDetailsAsync(userId, classId);
        return attendance == null ? NotFound() : Ok(attendance);
    }
}
