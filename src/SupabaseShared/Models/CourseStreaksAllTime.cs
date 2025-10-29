using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("CourseStreaksAllTime")]
public class CourseStreaksAllTime: BaseModel
{
    [Column]
    public Guid StudentId { get; set; }
    
    [Column]
    public string CourseCode { get; set; }
    
    [Column]
    public string FirstName { get; set; }
    
    [Column]
    public string LastName { get; set; }
    
    [Column]
    public int CourseYear { get; set; }

    [Column]
    public int CourseSemesterCode { get; set; }
    
    [Column]
    public int StreakLength { get; set; }
    
}