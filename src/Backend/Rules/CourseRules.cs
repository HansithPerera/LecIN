using System.Globalization;
using System.Text;
using Backend.Database;
using Backend.Dto.Req;
using Backend.Models;
using CsvHelper;
using ResultSharp;

namespace Backend.Rules;

public static class CourseRules
{
    public static Result<NewCourseReq, Errors.NewCourseError> IsValidCourse(NewCourseReq course)
    {
        if (string.IsNullOrWhiteSpace(course.Code))
            return Result.Err<NewCourseReq, Errors.NewCourseError>(Errors.NewCourseError.MissingCode);
        if (string.IsNullOrWhiteSpace(course.Name))
            return Result.Err<NewCourseReq, Errors.NewCourseError>(Errors.NewCourseError.MissingName);
        if (course.Year < 1 || course.Year > DateTime.Now.Year + 10)
            return Result.Err<NewCourseReq, Errors.NewCourseError>(Errors.NewCourseError.InvalidYear);
        if (course.SemesterCode < 1 || course.SemesterCode > 3)
            return Result.Err<NewCourseReq, Errors.NewCourseError>(Errors.NewCourseError.InvalidSemesterCode);
        return Result.Ok<NewCourseReq, Errors.NewCourseError>(course);
    }

    public static Result<Course, Errors.NewCourseError> CreateCourseFromReq(NewCourseReq courseReq)
    {
        var validation = IsValidCourse(courseReq);
        if (validation.IsErr) return Result.Err<Course, Errors.NewCourseError>(validation.UnwrapErr());

        return Result.Ok<Course, Errors.NewCourseError>(new Course
        {
            Code = courseReq.Code,
            Name = courseReq.Name,
            Year = courseReq.Year,
            SemesterCode = courseReq.SemesterCode
        });
    }

    public static async Task<Result<Stream, Errors.GetError>> ExportCourseAttendanceToCsv(string courseCode,
        int courseYear,
        int courseSemesterCode, AppService service)
    {
        var course = await service.GetCourseAsync(courseCode, courseYear, courseSemesterCode);
        if (course == null)
            return Result.Err<Stream, Errors.GetError>(Errors.GetError.NotFound);

        var attendanceData = await service.GetCourseAttendance(courseCode, courseYear, courseSemesterCode);

        var flattenedRecords = attendanceData.Select(a => Util.FlattenAttendance(a)).ToList();

        var stream = new MemoryStream();
        await using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            await csv.WriteRecordsAsync(flattenedRecords);
            await writer.FlushAsync();
        }

        stream.Position = 0;
        return Result.Ok<Stream, Errors.GetError>(stream);
    }

    public static async Task<Result<Course, Errors.NewCourseError>> AddNewCourse(NewCourseReq courseReq,
        AppService service)
    {
        var newCourseResult = CreateCourseFromReq(courseReq);
        if (newCourseResult.IsErr) return Result.Err<Course, Errors.NewCourseError>(newCourseResult.UnwrapErr());

        var newCourse = newCourseResult.Unwrap();

        var createdCourseResult = await service.AddCourseAsync(newCourse);
        if (!createdCourseResult.IsErr) return Result.Ok<Course, Errors.NewCourseError>(newCourse);

        var error = createdCourseResult.UnwrapErr();
        return error switch
        {
            Errors.InsertError.Conflict
                => Result.Err<Course, Errors.NewCourseError>(Errors.NewCourseError.Conflict),
            _
                => Result.Err<Course, Errors.NewCourseError>(Errors.NewCourseError.UnknownError)
        };
    }
}