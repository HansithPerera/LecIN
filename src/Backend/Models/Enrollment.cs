using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Enrollments")]
public class Enrollment
{
    [MaxLength(255)] public Guid StudentId { get; set; }

    public Student? Student { get; set; }

    public Course? Course { get; set; }

    [MaxLength(255)] public required string CourseCode { get; set; }

    [MaxLength(255)] public required string CourseId { get; set; }

    public int CourseYearId { get; set; }

    public int CourseSemesterCode { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}