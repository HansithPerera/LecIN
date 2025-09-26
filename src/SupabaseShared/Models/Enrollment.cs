using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Enrollments")]
public class Enrollment: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid StudentId { get; set; }

    [Column]
    public string CourseCode { get; set; }

    [Column]
    public string CourseId { get; set; }

    [Column]
    public int CourseYear { get; set; }

    [Column]
    public int CourseSemesterCode { get; set; }

    [Column]
    public DateTimeOffset EnrolledAt { get; set; }
}