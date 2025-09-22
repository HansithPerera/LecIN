using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("Attendance")]
public class Attendance: BaseModel
{
    [PrimaryKey]
    public Guid StudentId { get; set; }

    [PrimaryKey]
    public Guid ClassId { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}