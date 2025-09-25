namespace Backend.Dto;
public record AttendancePercentResponse(
    Guid StudentId,
    int Attended,
    int Total,
    double Percent,
    DateTimeOffset AsOf
);