[HttpPut("course/{courseId}/classes/{classId}/attendance/{studentId}")]
[ProducesResponseType(typeof(Attendance), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> UpdateStudentAttendance(
    int courseId,
    int classId,
    int studentId,
    [FromBody] Attendance updatedAttendance)
{
    if (updatedAttendance == null)
        return BadRequest("Invalid attendance data.");

    var teacherId = User.Identity?.Name;
    if (string.IsNullOrEmpty(teacherId))
        return NotFound("User ID not found in token.");

    // Check if teacher has access to the course
    var teacher = await repository.GetTeacherByIdAsync(teacherId);
    if (teacher == null)
        return NotFound("Teacher not found.");

    var isEnrolled = await repository.IsTeacherEnrolledInCourseAsync(teacherId, courseId);
    if (!isEnrolled)
        return Forbid("Teacher does not have access to this course.");

    // Update the attendance record
    var result = await repository.UpdateStudentAttendanceAsync(courseId, classId, studentId, updatedAttendance);
    if (result == null)
        return NotFound("Attendance record not found or could not be updated.");

    return Ok(result);
}
