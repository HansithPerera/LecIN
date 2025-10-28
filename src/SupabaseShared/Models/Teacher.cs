using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Teachers")]
public class Teacher : BaseModel
{
    [PrimaryKey()] public Guid Id { get; set; }

    [Column()] public string FirstName { get; set; } = string.Empty;

    [Column()] public string LastName { get; set; } = string.Empty;

    [Column()] public DateTime CreatedAt { get; set; }

    [Column()] public DateTime? UpdatedAt { get; set; }
}