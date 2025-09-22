using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("StudentFaces")]
public class StudentFace : BaseModel
{
    [PrimaryKey]
    public Guid StudentId { get; set; }

    [Column]
    public string FaceImagePath { get; set; }

    [Column]
    public DateTimeOffset CreatedAt { get; set; }

    [Column]
    public DateTimeOffset? UpdatedAt { get; set; }
    
    [Reference(typeof(Student))]
    public Student Student { get; set; }
}