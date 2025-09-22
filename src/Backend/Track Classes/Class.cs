using System;
using System.Collections.Generic;

public class Class
{
    public int Id { get; set; }
    public string ClassName { get; set; }
    public DateTime StartTime { get; set; }

    public List<Attendance> AttendanceRecords { get; set; }
}
