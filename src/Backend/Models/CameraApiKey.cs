using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum ApiKeyRole
{
    Primary,
    Secondary
}

[Table("CameraApiKeys")]
public class CameraApiKey
{
    [ForeignKey(nameof(Camera))] public required Guid CameraId { get; set; }

    public Camera? Camera { get; set; }

    public required Guid ApiKeyId { get; set; }

    public required ApiKeyRole Role { get; set; }
}