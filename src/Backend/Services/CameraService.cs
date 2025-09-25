using Backend.Database;
using Backend.Face;
using Backend.Models;
using ResultSharp;

namespace Backend.Services;

public class CameraService(Repository repo, FaceService faceService)
{
    public async Task<Result<RecognizedFace, Errors.FaceRecognitionError>> AnalyzeFace(Stream imageStream)
    {
        try
        {
            var recognizedFaces = await faceService.RecognizeFacesAsync(imageStream);
            return recognizedFaces.Count switch
            {
                0 => Result.Err<RecognizedFace, Errors.FaceRecognitionError>(
                    Errors.FaceRecognitionError.NoFaceDetected),
                > 1 => Result.Err<RecognizedFace, Errors.FaceRecognitionError>(
                    Errors.FaceRecognitionError.MultipleFacesDetected),
                _ => Result.Ok<RecognizedFace, Errors.FaceRecognitionError>(
                    recognizedFaces.First())
            };
        }
        catch (FormatException)
        {
            return Result.Err<RecognizedFace, Errors.FaceRecognitionError>(Errors.FaceRecognitionError
                .InvalidImageFormat);
        }
        catch (Exception)
        {
            return Result.Err<RecognizedFace, Errors.FaceRecognitionError>(Errors.FaceRecognitionError.UnknownError);
        }
    }

    public static async Task<Result<Class, Errors.ClassRetrievalError>> GetOngoingClass(Guid apiKeyId,
        Repository service)
    {
        var camera = await service.GetCameraByApiKeyIdAsync(apiKeyId);
        if (camera == null)
            return Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoCameraFound);
        var classroom = await service.GetClassByLocationTimeAsync(camera.Location, DateTime.UtcNow);
        return classroom == null
            ? Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoClassFound)
            : Result.Ok<Class, Errors.ClassRetrievalError>(classroom);
    }

    public async Task<Result<Attendance, Errors.CheckInError>> CheckFaceIntoClass(Guid classId, Stream imageStream)
    {
        var classroom = await repo.GetClassByIdAsync(classId);
        if (classroom == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.ClassNotFound);
        var analysisResult = await AnalyzeFace(imageStream);
        if (analysisResult.IsErr)
            return Result.Err<Attendance, Errors.CheckInError>(
                analysisResult.UnwrapErr() == Errors.FaceRecognitionError.UnknownError
                    ? Errors.CheckInError.UnknownError
                    : Errors.CheckInError.FaceRecognitionFailed
            );

        var recognizedFace = analysisResult.Unwrap();
        var student = await repo.GetStudentByFaceId(recognizedFace.PersonId);
        if (student == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotFound);

        var enrolled = await repo.IsStudentEnrolledInCourseAsync(student.Id, classroom.CourseCode,
            classroom.CourseYear, classroom.CourseSemesterCode);
        if (!enrolled) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotEnrolled);

        var attendance = await repo.CreateAttendanceAsync(student.Id, classroom.Id);
        return Result.Ok<Attendance, Errors.CheckInError>(attendance);
    }
}