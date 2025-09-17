namespace Backend.Models;

public class CameraApiKey
{
    public required Guid CameraId { get; set; }
    
    public required Guid ApiKeyId { get; set; }
}