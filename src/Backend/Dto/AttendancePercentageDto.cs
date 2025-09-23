namespace Backend.Dto;

public record CourseAttendanceDto
    (
    string CourseCode,
    int TotalClasses,
    int Attended,
    double Percentage); // e.g., 83.3);

public record AttendancePercentageDto
    (
    Guid StudentId,
    int TotalClasses,
    int Attended,
    double Percentage, // overall
    List<CourseAttendanceDto> ByCourse);

