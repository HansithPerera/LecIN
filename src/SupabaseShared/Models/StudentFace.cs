using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("StudentFaces")]
public class StudentFace : BaseModel
{
    [PrimaryKey(shouldInsert: true)] public Guid StudentId { get; set; }

    [Column] public float[] Embedding { get; set; }

    [Column] public DateTimeOffset CreatedAt { get; set; }

    [Column] public DateTimeOffset? UpdatedAt { get; set; }

    [Reference(typeof(Student))] public Student? Student { get; set; }
}