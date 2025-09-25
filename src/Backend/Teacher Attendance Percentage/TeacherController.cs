using System.Threading.Tasks;

[HttpGet("student/{studentId}/attendance-percentage")]
[ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetStudentAttendancePercentage(string studentId)
{
    var student = await repository.GetStudentByIdAsync(studentId);
    if (student == null)
        return NotFound("Student not found.");

    var percentage = await repository.GetStudentAttendancePercentageAsync(studentId);
    return Ok(new
    {
        StudentId = studentId,
        AttendancePercentage = percentage,
        Message = $"Overall Attendance: {percentage}%"
    });
}
