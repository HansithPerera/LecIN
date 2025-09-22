using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Constants.StudentAuthorizationPolicy)]
public class StudentController(AppService repository) : ControllerBase
{
    [HttpGet("attendance/history")]
    [ProducesResponseType(typeof(List<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttendanceHistory()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return NotFound("User ID not found in token.");

        var attendanceHistory = await repository.GetAttendanceHistoryForStudentAsync(userId);
        return Ok(attendanceHistory);
    }
}
