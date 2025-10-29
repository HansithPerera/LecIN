using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

public enum ApiKeyRole
{
    Primary,
    Secondary
}

[Table("CameraApiKeys")]
public class CameraApiKey: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid CameraId { get; set; }

    [Column]
    public Guid ApiKeyId { get; set; }

    [Column]
    public ApiKeyRole Role { get; set; }
    
    [Reference(typeof(Camera))]
    public Camera? Camera { get; set; }
}