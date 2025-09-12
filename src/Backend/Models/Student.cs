using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Students")]
public class Student
{
    [Key] [MaxLength(255)] public required string Id { get; set; }

    [MaxLength(255)] public required string FirstName { get; set; }

    [MaxLength(255)] public required string LastName { get; set; }

    public List<Enrollment>? Enrollments { get; set; }
}