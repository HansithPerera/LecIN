using System;
using System.Threading.Tasks;

public async Task<string> RecordAttendanceAsync(int studentId, int classId)
{
    var student = await dbContext.Students.FindAsync(studentId);
    if (student == null) return "Student not found.";

    var existing = await dbContext.Attendance
        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.ClassId == classId);
    if (existing != null) return "Attendance already recorded.";

    // Record attendance
    var attendance = new Attendance
    {
        StudentId = studentId,
        ClassId = classId,
        Status = "Present",
        Timestamp = DateTime.UtcNow
    };

    dbContext.Attendance.Add(attendance);

    // Streak check
    bool previousClassMissed = await CheckPreviousClassMissedAsync(studentId, classId);
    if (previousClassMissed)
    {
        student.AttendanceStreak = 1; // Reset to 1
    }
    else
    {
        student.AttendanceStreak += 1;
    }

    // Badge logic
    if (student.AttendanceStreak == 5)
    {
        student.Badges.Add("5-Day Streak");
        // You can also add a notification log here
    }

    await dbContext.SaveChangesAsync();
    return "Attendance recorded successfully.";
}
