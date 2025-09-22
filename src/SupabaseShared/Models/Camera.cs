using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("Cameras")]
public class Camera: BaseModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public string Location { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}