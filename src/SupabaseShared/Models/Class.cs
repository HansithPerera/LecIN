using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("Classes")]
public class Class: BaseModel
{
    [PrimaryKey]
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

    [Column]
    public TimeSpan Duration => EndTime - StartTime;

    [Column]
    public DateTimeOffset EndTime { get; set; }
}