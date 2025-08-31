using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Enrollments")]
public class Enrollment
{
    [MaxLength(255)]
    [ForeignKey(nameof(Student))]
    public required string StudentId { get; set; }

    public required Student Student { get; set; }

    [MaxLength(255)] public required string CourseCode { get; set; }

    [MaxLength(255)] public required string CourseId { get; set; }

    public int CourseYearId { get; set; }

    public int CourseSemesterCode { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}