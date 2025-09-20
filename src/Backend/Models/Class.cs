using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Classes")]
public class Class
{
    [MaxLength(255)] [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)] public required string CourseCode { get; set; }

    public int CourseYearId { get; set; }

    public int CourseSemesterCode { get; set; }

    [MaxLength(255)] public string? Location { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public TimeSpan Duration => EndTime - StartTime;

    public DateTimeOffset EndTime { get; set; }
}