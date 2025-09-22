using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Backend.Api.Models;

[Table("Students")]
public class Student: BaseModel
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column]
    public string FirstName { get; set; }

    [Column]
    public string LastName { get; set; }
}