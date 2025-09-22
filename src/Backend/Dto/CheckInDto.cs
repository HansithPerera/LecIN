namespace Backend.Dto;

public record CheckInRequest ( Guid ClassId);

public record CheckInResponse(
    
    string Message,
    string CourseCode,
    DateTimeOffset Timestamp,
    string status // present || late

    );
