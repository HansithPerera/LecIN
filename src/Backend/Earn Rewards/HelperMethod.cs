using System.Threading.Tasks;

private async Task<bool> CheckPreviousClassMissedAsync(int studentId, int currentClassId)
{
    // Fetch the previous class (assumes ordered class schedule)
    var previousClass = await dbContext.Classes
        .Where(c => c.Id < currentClassId)
        .OrderByDescending(c => c.Id)
        .FirstOrDefaultAsync();

    if (previousClass == null)
        return false; // No previous class to check

    var previousAttendance = await dbContext.Attendance
        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.ClassId == previousClass.Id);

    return previousAttendance == null || previousAttendance.Status == "Absent";
}
