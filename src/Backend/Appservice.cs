using System.Collections.Generic;
using System.Threading.Tasks;

public async Task<List<AttendanceSummaryDto>> GetStudentAttendanceHistoryAsync(string userId)
{
    var student = await _context.Students
        .Include(s => s.AttendanceHistory)
            .ThenInclude(a => a.Class)
        .FirstOrDefaultAsync(s => s.Id == userId);

    if (student == null) return [];

    return student.AttendanceHistory.Select(a => new AttendanceSummaryDto
    {
        ClassDate = a.Class.Date,
        Status = a.Status.ToString()
    }).OrderBy(a => a.ClassDate).ToList();
}

public async Task<AttendanceDetailDto?> GetAttendanceDetailsAsync(string userId, int classId)
{
    var attendance = await _context.Attendances
        .Include(a => a.Class)
        .ThenInclude(c => c.Course)
        .FirstOrDefaultAsync(a => a.StudentId == userId && a.ClassId == classId);

    if (attendance == null) return null;

    return new AttendanceDetailDto
    {
        CourseName = attendance.Class.Course.Name,
        ClassTime = attendance.Class.Date,
        Status = attendance.Status.ToString()
    };
}
