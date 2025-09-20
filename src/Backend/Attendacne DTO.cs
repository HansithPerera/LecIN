public class AttendanceSummaryDto
{
    public DateTime ClassDate { get; set; }
    public string Status { get; set; }
}

public class AttendanceDetailDto
{
    public string CourseName { get; set; }
    public DateTime ClassTime { get; set; }
    public string Status { get; set; }
}
