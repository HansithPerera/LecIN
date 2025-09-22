using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("ApiKeys")]
public class ApiKey: BaseModel
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string Hash { get; set; }

    [Column]
    public bool IsActive { get; set; }

    [Column]
    public string? Name { get; set; }

    [Column]
    public string Prefix { get; set; }

    [Column]
    public DateTimeOffset CreatedAt { get; set; }
}