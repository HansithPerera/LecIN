using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models;

[Table("CourseTeachers")]
public class CourseTeacher : BaseModel
{
    [Column("TeacherId")]
    public Guid TeacherId { get; set; }

    [Column("CourseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [Column("CourseSemesterCode")]
    public int CourseSemesterCode { get; set; }

    [Column("CourseYear")]
    public int CourseYear { get; set; }
}
