using System;

public class AttendanceDto
{
    public string ClassName { get; set; }
    public DateTime ClassStartTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusIcon => Status switch
    {
        AttendanceStatus.Present => "✅",
        AttendanceStatus.Late => "⏰",
        AttendanceStatus.Absent => "❌",
        AttendanceStatus.Excused => "📝",
        _ => ""
    };
}
