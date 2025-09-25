using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Attendance")]
public class Attendance: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid StudentId { get; set; }

    [PrimaryKey]
    public Guid ClassId { get; set; }
    
    [Column]
    public string? Reason { get; set; }

    [Column]
    public DateTimeOffset Timestamp { get; set; }
}