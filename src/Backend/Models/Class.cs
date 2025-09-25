using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Classes")]
public class Class
{
    [MaxLength(255)] [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)] public required string CourseCode { get; set; }

    [Column("CourseYear")]
    public int CourseYear { get; set; }

    [Column("CourseSemesterCode")]
    public int CourseSemesterCode { get; set; }

    [MaxLength(255)] public string? Location { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public TimeSpan Duration => EndTime - StartTime;

    public DateTimeOffset EndTime { get; set; }
}