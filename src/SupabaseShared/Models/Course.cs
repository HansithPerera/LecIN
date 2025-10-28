using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Courses")]
public class Course: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public string Code { get; set; } = string.Empty;

    [PrimaryKey(shouldInsert: true)]
    public int Year { get; set; }

    [PrimaryKey(shouldInsert: true)]
    public int SemesterCode { get; set; }

    [Column]
    public string Name { get; set; } = string.Empty;
}