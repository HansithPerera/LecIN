using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models;

[Table("Teachers")]
public class Teacher : BaseModel
{
    [PrimaryKey("Id")]
    public Guid Id { get; set; }

    [Column("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [Column("LastName")]
    public string LastName { get; set; } = string.Empty;

    [Column("Email")]
    public string Email { get; set; } = string.Empty;
}
