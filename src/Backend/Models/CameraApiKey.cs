using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

public enum ApiKeyRole
{
    Primary,
    Secondary
}

[Table("CameraApiKeys")]
[PrimaryKey(nameof(CameraId), nameof(Role))]
[Index(nameof(ApiKeyId), IsUnique = true)]
public class CameraApiKey
{
    [ForeignKey("CameraApiKeyId")] public Guid CameraId { get; set; }

    public Guid ApiKeyId { get; set; }

    public ApiKeyRole Role { get; set; }

    public Camera? Camera { get; set; }
}