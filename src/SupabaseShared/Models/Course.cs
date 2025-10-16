using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Courses")]
public class Course: BaseModel
{
    public string Code { get; set; } = string.Empty;

    public int Year { get; set; }

    public int SemesterCode { get; set; }

    public string Name { get; set; } = string.Empty;
}