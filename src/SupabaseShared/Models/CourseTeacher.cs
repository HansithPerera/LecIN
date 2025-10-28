using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("CourseTeachers")]
public class CourseTeacher : BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid TeacherId { get; set; }

    [PrimaryKey(shouldInsert: true)]
    public string CourseCode { get; set; } = string.Empty;

    [PrimaryKey(shouldInsert: true)]
    public int CourseYear { get; set; }

    [PrimaryKey(shouldInsert: true)]
    public int CourseSemesterCode { get; set; }

    [Reference(typeof(Teacher))]
    public Teacher? Teacher { get; set; }

    [Reference(typeof(Course))]
    public Course? Course { get; set; }
}
