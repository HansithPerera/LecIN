using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Cameras")]
public class Camera: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string Name { get; set; }

    [Column]
    public string Location { get; set; }

    [Column]
    public bool IsActive { get; set; }

    [Column]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column]
    public DateTimeOffset? UpdatedAt { get; set; }
}