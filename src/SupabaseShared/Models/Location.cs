using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Locations")]
public class Location: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public string Id { get; set; }

    [Column]
    public string Room { get; set; } = string.Empty;
    
    [Column]
    public string Level { get; set; } = string.Empty;
    
    [Column]
    public string Building { get; set; } = string.Empty;
    
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
    
    [Column]
    public string Address { get; set; } = string.Empty;
    
    [Column]
    public string Coords { get; set; } = string.Empty;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}