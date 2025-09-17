using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Teachers")]
public class Teacher
{
    [MaxLength(255)] public required Guid Id { get; set; }

    [MaxLength(255)] public required string FirstName { get; set; }

    [MaxLength(255)] public required string LastName { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}