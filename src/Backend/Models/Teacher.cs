using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Teachers")]
public class Teacher
{
    [Key] public required string Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}