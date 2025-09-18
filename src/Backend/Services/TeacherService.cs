using Backend.Database;
using Backend.Dto.Req;
using Backend.Models;
using ResultSharp;

namespace Backend.Services;

public class TeacherService(Repository repo)
{
    public static Result<NewTeacherReq, Errors.NewTeacherError> IsValidTeacher(NewTeacherReq teacher)
    {
        if (string.IsNullOrWhiteSpace(teacher.FirstName))
            return Result.Err<NewTeacherReq, Errors.NewTeacherError>(
                Errors.NewTeacherError.MissingFirstName
            );
        if (string.IsNullOrWhiteSpace(teacher.LastName))
            return Result.Err<NewTeacherReq, Errors.NewTeacherError>(
                Errors.NewTeacherError.MissingLastName
            );
        return Result.Ok<NewTeacherReq, Errors.NewTeacherError>(teacher);
    }

    public static Result<Teacher, Errors.NewTeacherError> CreateTeacherFromReq(NewTeacherReq teacherReq)
    {
        var validation = IsValidTeacher(teacherReq);
        if (validation.IsErr) return Result.Err<Teacher, Errors.NewTeacherError>(validation.UnwrapErr());

        return Result.Ok<Teacher, Errors.NewTeacherError>(new Teacher
        {
            Id = Guid.NewGuid(),
            FirstName = teacherReq.FirstName,
            LastName = teacherReq.LastName,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task<Result<Teacher, Errors.NewTeacherError>> AddNewTeacherAsync(NewTeacherReq teacherReq)
    {
        var newTeacherResult = CreateTeacherFromReq(teacherReq);
        if (newTeacherResult.IsErr) return Result.Err<Teacher, Errors.NewTeacherError>(newTeacherResult.UnwrapErr());

        var newTeacher = newTeacherResult.Unwrap();

        var createdTeacherResult = await repo.AddTeacherAsync(newTeacher);
        if (createdTeacherResult.IsOk) return Result.Ok<Teacher, Errors.NewTeacherError>(newTeacher);

        var error = createdTeacherResult.UnwrapErr();
        return error switch
        {
            Errors.InsertError.Conflict
                => Result.Err<Teacher, Errors.NewTeacherError>(Errors.NewTeacherError.Conflict),
            _
                => Result.Err<Teacher, Errors.NewTeacherError>(Errors.NewTeacherError.UnknownError)
        };
    }
}