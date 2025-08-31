using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Courses")]
public class Course
{
    [MaxLength(255)] public required string Code { get; set; }

    public int Year { get; set; }

    public int SemesterCode { get; set; }

    [MaxLength(255)] public required string Name { get; set; }
}