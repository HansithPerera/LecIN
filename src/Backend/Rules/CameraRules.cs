using Backend.Database;
using Backend.Models;
using ResultSharp;

namespace Backend.Rules;

public class CameraRules
{
    
    public enum FaceRecognitionError
    {
        NoFaceDetected,
        MultipleFacesDetected,
        RecognitionFailed,
        InvalidImageFormat,
        UnknownError
    }
    
    public class RecognizedFace
    {
        public required string IdentifierId { get; set; }
        public required float Confidence { get; set; }
    }
    
    public static async Task<bool> IsCameraAuthorized(string apiKeyHash, AppService service)
    {
        var camera = await service.GetCameraByApiKeyHashAsync(apiKeyHash);
        return camera is { IsActive: true };
    }

    
    public static async Task<Result<RecognizedFace, FaceRecognitionError>> AnalyzeFace(Stream imageStream)
    {
        throw new NotImplementedException();   
    }
    
    public static async Task<Result<bool, FaceRecognitionError>> TryCheckinStudent(Stream imageStream)
    {
        throw new NotImplementedException();
    }
    
}