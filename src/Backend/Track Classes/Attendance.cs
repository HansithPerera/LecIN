using System;

public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public AttendanceStatus Status { get; set; } // Present, Absent, Late, Excused
    public DateTime Timestamp { get; set; }

    public Class ClassInfo { get; set; }
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    Excused
}
