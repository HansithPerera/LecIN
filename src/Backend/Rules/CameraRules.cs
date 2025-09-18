using Backend.Database;
using Backend.Face;
using Backend.Models;
using ResultSharp;

namespace Backend.Rules;

public class CameraRules
{
    public enum CheckInError
    {
        FaceRecognitionFailed,
        StudentNotFound,
        AlreadyCheckedIn,
        ClassNotFound,
        StudentNotEnrolled,
        UnknownError
    }

    public enum ClassRetrievalError
    {
        NoCameraFound,
        NoClassFound,
        UnknownError
    }

    public enum FaceRecognitionError
    {
        NoFaceDetected,
        MultipleFacesDetected,
        RecognitionFailed,
        InvalidImageFormat,
        UnknownError
    }

    public static async Task<Result<RecognizedFace, FaceRecognitionError>> AnalyzeFace(Stream imageStream,
        FaceService service)
    {
        try
        {
            var recognizedFaces = await service.RecognizeFacesAsync(imageStream);
            return recognizedFaces.Count switch
            {
                0 => Result.Err<RecognizedFace, FaceRecognitionError>(
                    FaceRecognitionError.NoFaceDetected),
                > 1 => Result.Err<RecognizedFace, FaceRecognitionError>(
                    FaceRecognitionError.MultipleFacesDetected),
                _ => Result.Ok<RecognizedFace, FaceRecognitionError>(
                    recognizedFaces.First())
            };
        }
        catch (FormatException)
        {
            return Result.Err<RecognizedFace, FaceRecognitionError>(FaceRecognitionError.InvalidImageFormat);
        }
        catch (Exception)
        {
            return Result.Err<RecognizedFace, FaceRecognitionError>(FaceRecognitionError.UnknownError);
        }
    }


    public static async Task<Result<Class, ClassRetrievalError>> GetOngoingClass(Guid apiKeyId, AppService service)
    {
        var camera = await service.GetCameraByApiKeyIdAsync(apiKeyId);
        if (camera == null) return Result.Err<Class, ClassRetrievalError>(ClassRetrievalError.NoCameraFound);
        var classroom = await service.GetClassByLocationTimeAsync(camera.Location, DateTime.UtcNow);
        return classroom == null
            ? Result.Err<Class, ClassRetrievalError>(ClassRetrievalError.NoClassFound)
            : Result.Ok<Class, ClassRetrievalError>(classroom);
    }

    public static async Task<Result<Attendance, CheckInError>> CheckFaceIntoClass(Guid classId, Stream imageStream,
        AppService service, FaceService faceService)
    {
        var classroom = await service.GetClassByIdAsync(classId);
        if (classroom == null) return Result.Err<Attendance, CheckInError>(CheckInError.ClassNotFound);
        var analysisResult = await AnalyzeFace(imageStream, faceService);
        if (analysisResult.IsErr)
            return Result.Err<Attendance, CheckInError>(
                analysisResult.UnwrapErr() == FaceRecognitionError.UnknownError
                    ? CheckInError.UnknownError
                    : CheckInError.FaceRecognitionFailed
            );

        var recognizedFace = analysisResult.Unwrap();
        var student = await service.GetStudentByFaceId(recognizedFace.PersonId);
        if (student == null) return Result.Err<Attendance, CheckInError>(CheckInError.StudentNotFound);

        var enrolled = await service.IsStudentEnrolledInCourseAsync(student.Id, classroom.CourseCode,
            classroom.CourseYearId, classroom.CourseSemesterCode);
        if (!enrolled) return Result.Err<Attendance, CheckInError>(CheckInError.StudentNotEnrolled);

        var attendance = await service.CreateAttendanceAsync(student.Id, classroom.Id);
        return Result.Ok<Attendance, CheckInError>(attendance);
    }
}