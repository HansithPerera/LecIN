using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("ApiKeys")]
public class ApiKey: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string Hash { get; set; } = string.Empty;

    [Column]
    public bool IsActive { get; set; }

    [Column]
    public string? Name { get; set; }

    [Column]
    public string Prefix { get; set; } = string.Empty;

    [Column]
    public DateTimeOffset CreatedAt { get; set; }
}