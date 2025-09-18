using Backend.Database;
using Backend.Face;
using Backend.Models;
using ResultSharp;

namespace Backend.Rules;

public class CameraRules
{
    
    public static Result<Camera, Errors.NewCameraError> ValidateNewCamera(string name, string location)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Err<Camera, Errors.NewCameraError>(Errors.NewCameraError.MissingName);
        if (string.IsNullOrWhiteSpace(location) || location.Length < 3)
            return Result.Err<Camera, Errors.NewCameraError>(Errors.NewCameraError.MissingLocation);
        return Result.Ok<Camera, Errors.NewCameraError>(new Camera { Name = name, Location = location });
    }
    
    public static async Task<Result<Camera, Errors.NewCameraError>> CreateCameraAsync(string name, string location, AppService service)
    {
        var validationResult = ValidateNewCamera(name, location);
        if (validationResult.IsErr) 
            return validationResult;
        
        var camera = validationResult.Unwrap();
        await service.CreateCameraAsync(camera);
        return Result.Ok<Camera, Errors.NewCameraError>(camera);
    }

    public static async Task<Result<RecognizedFace, Errors.FaceRecognitionError>> AnalyzeFace(Stream imageStream,
        FaceService service)
    {
        try
        {
            var recognizedFaces = await service.RecognizeFacesAsync(imageStream);
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
            return Result.Err<RecognizedFace, Errors.FaceRecognitionError>(Errors.FaceRecognitionError.InvalidImageFormat);
        }
        catch (Exception)
        {
            return Result.Err<RecognizedFace, Errors.FaceRecognitionError>(Errors.FaceRecognitionError.UnknownError);
        }
    }


    public static async Task<Result<Class, Errors.ClassRetrievalError>> GetOngoingClass(Guid apiKeyId, AppService service)
    {
        var camera = await service.GetCameraByApiKeyIdAsync(apiKeyId);
        if (camera == null) return Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoCameraFound);
        var classroom = await service.GetClassByLocationTimeAsync(camera.Location, DateTime.UtcNow);
        return classroom == null
            ? Result.Err<Class, Errors.ClassRetrievalError>(Errors.ClassRetrievalError.NoClassFound)
            : Result.Ok<Class, Errors.ClassRetrievalError>(classroom);
    }

    public static async Task<Result<Attendance, Errors.CheckInError>> CheckFaceIntoClass(Guid classId, Stream imageStream,
        AppService service, FaceService faceService)
    {
        var classroom = await service.GetClassByIdAsync(classId);
        if (classroom == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.ClassNotFound);
        var analysisResult = await AnalyzeFace(imageStream, faceService);
        if (analysisResult.IsErr)
            return Result.Err<Attendance, Errors.CheckInError>(
                analysisResult.UnwrapErr() == Errors.FaceRecognitionError.UnknownError
                    ? Errors.CheckInError.UnknownError
                    : Errors.CheckInError.FaceRecognitionFailed
            );

        var recognizedFace = analysisResult.Unwrap();
        var student = await service.GetStudentByFaceId(recognizedFace.PersonId);
        if (student == null) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotFound);

        var enrolled = await service.IsStudentEnrolledInCourseAsync(student.Id, classroom.CourseCode,
            classroom.CourseYearId, classroom.CourseSemesterCode);
        if (!enrolled) return Result.Err<Attendance, Errors.CheckInError>(Errors.CheckInError.StudentNotEnrolled);

        var attendance = await service.CreateAttendanceAsync(student.Id, classroom.Id);
        return Result.Ok<Attendance, Errors.CheckInError>(attendance);
    }
    
    public class GeneratedApiKey
    {
        public required CameraApiKey CameraApiKey { get; set; }
        public required string UnhashedKey { get; set; }
    }

    public static async Task<Result<GeneratedApiKey, Errors.GenerateCameraApiKeyError>> RegenerateCameraApiKeyAsync(Guid cameraId, ApiKeyRole role, AppService service)
    {
        var camera = await service.GetCameraByIdAsync(cameraId);
        if (camera == null)
            return Result.Err<GeneratedApiKey, Errors.GenerateCameraApiKeyError>(Errors.GenerateCameraApiKeyError.CameraNotFound);

        var newApiKey = ApiKey.Create(out var unhashedKey, "Camera API Key");
        newApiKey = await service.CreateApiKeyAsync(newApiKey);
        var generatedCameraKey = await service.SetCameraApiKeyAsync(cameraId, newApiKey.Id, ApiKeyRole.Primary);
        generatedCameraKey.ApiKey = newApiKey;
        return Result.Ok<GeneratedApiKey, Errors.GenerateCameraApiKeyError>(new GeneratedApiKey
        {
            CameraApiKey = generatedCameraKey,
            UnhashedKey = unhashedKey
        });
    }
}