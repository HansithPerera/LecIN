using System.Collections.Generic;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string StudentEmail { get; set; }
    public string StudentId { get; set; }
    public string FacialDataReference { get; set; }
    public List<Attendance> AttendanceHistory { get; set; }

    // New fields for streaks
    public int AttendanceStreak { get; set; }
    public List<string> Badges { get; set; } = new();
}
