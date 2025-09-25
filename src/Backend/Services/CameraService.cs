using Backend.Database;
using Backend.Face;
using ResultSharp;
using SupabaseShared.Models;

namespace Backend.Services;

public class CameraService(Repository repo, FaceService faceService)
{
    private async Task<Result<Class, Errors.ClassRetrievalError>> GetOngoingClass(Guid apiKeyId)
    {
        var camera = await repo.GetCameraByApiKeyIdAsync(apiKeyId);
        if (camera == null)
            return Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoCameraFound);
        var classroom = await repo.GetClassByLocationTimeAsync(camera.Location, DateTime.UtcNow);
        return classroom == null
            ? Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoClassFound)
            : Result.Ok<Class, Errors.ClassRetrievalError>(classroom);
    }
    
    public async Task<Result<Attendance, Errors.CheckInError>> CheckFaceIntoClass(Guid apiKeyId, Stream imageStream)
    {
        var classResult = await GetOngoingClass(apiKeyId);
        if (classResult.IsErr)
            return classResult.UnwrapErr() switch
            {
                Errors.ClassRetrievalError.NoCameraFound =>
                    Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.NoCameraFound),
                Errors.ClassRetrievalError.NoClassFound =>
                    Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.ClassNotFound),
                _ => Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.UnknownError)
            };
        
        var classValue = classResult.Unwrap();
        
        var recognizedFace = await faceService.RecognizeFaceAsync(imageStream);
        if (recognizedFace == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.FaceRecognitionFailed);
        
        var student = await repo.GetStudentById(recognizedFace.PersonId);
        if (student == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotFound);

        var enrolled = await repo.IsStudentEnrolledInCourseAsync(
            student.Id, 
            classValue.CourseCode,
            classValue.CourseYear, 
            classValue.CourseSemesterCode);
        
        if (!enrolled)
            return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotEnrolled);
        
        var attendance = await repo.UpsertAttendanceAsync(student.Id, classValue.Id);
        
        return Result.Ok<Attendance, Errors.CheckInError>(attendance);
    }
}