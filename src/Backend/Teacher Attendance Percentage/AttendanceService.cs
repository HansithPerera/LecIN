public async Task<double?> GetStudentAttendancePercentageAsync(string studentId)
{
    // Get all classes the student was expected to attend
    var totalClasses = await _context.Attendances
        .Where(a => a.StudentId == studentId)
        .CountAsync();

    if (totalClasses == 0)
        return 0;

    // Count where the student was present or late (optional: exclude Excused if not counted)
    var attendedClasses = await _context.Attendances
        .Where(a => a.StudentId == studentId &&
               (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
        .CountAsync();

    double percentage = (double)attendedClasses / totalClasses * 100;
    return Math.Round(percentage, 2); // 2 decimal places
}
