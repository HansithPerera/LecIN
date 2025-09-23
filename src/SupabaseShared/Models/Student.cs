using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseShared.Models;

[Table("Students")]
public class Student: BaseModel
{
    [PrimaryKey(shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string FirstName { get; set; }

    [Column]
    public string LastName { get; set; }
}