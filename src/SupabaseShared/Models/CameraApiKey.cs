using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

public enum ApiKeyRole
{
    Primary,
    Secondary
}

[Table("CameraApiKeys")]
public class CameraApiKey: BaseModel
{
    [Column]
    public Guid CameraId { get; set; }

    [Column]
    public Guid ApiKeyId { get; set; }

    [Column]
    public ApiKeyRole Role { get; set; }
    
    public Camera? Camera { get; set; }
}