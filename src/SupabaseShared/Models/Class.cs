using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Classes")]
public class Class: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string CourseCode { get; set; }

    [Column]
    public int CourseYear { get; set; }

    [Column]
    public int CourseSemesterCode { get; set; }

    [Column]
    public string? Location { get; set; }

    [Column]
    public DateTimeOffset StartTime { get; set; }
    
    public TimeSpan Duration => EndTime - StartTime;

    [Column]
    public DateTimeOffset EndTime { get; set; }
}