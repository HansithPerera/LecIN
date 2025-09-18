using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum KeyRole
{
    Primary,
    Secondary
}

[Table("CameraApiKeys")]
public class CameraApiKey
{
    [ForeignKey(nameof(Camera))]
    public required Guid CameraId { get; set; }
    
    public Camera? Camera { get; set; }
    
    public required Guid ApiKeyId { get; set; }
    
    public required KeyRole Role { get; set; }
}