using Backend.Database;
using Backend.Dto.Req;
using Backend.Models;
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
    
    public static async Task<Result<Course, Errors.NewCourseError>> AddNewCourse(NewCourseReq courseReq, AppService service)
    {
        var newCourseResult = CreateCourseFromReq(courseReq);
        if (newCourseResult.IsErr) return Result.Err<Course, Errors.NewCourseError>(newCourseResult.UnwrapErr());

        var newCourse = newCourseResult.Unwrap();
        
        var createdCourseResult = await service.AddCourseAsync(newCourse);
        if (!createdCourseResult.IsErr)
        {
            return Result.Ok<Course, Errors.NewCourseError>(newCourse);
        }
        
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