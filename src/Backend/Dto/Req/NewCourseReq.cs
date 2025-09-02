namespace Backend.Dto.Req;

public class NewCourseReq
{
    public required string Code { get; set; }

    public required int Year { get; set; }

    public required int SemesterCode { get; set; }

    public required string Name { get; set; }
}