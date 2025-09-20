using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("Enrollments")]
[PrimaryKey(nameof(StudentId), nameof(CourseCode), nameof(CourseYearId), nameof(CourseSemesterCode))]
public class Enrollment
{
    [MaxLength(255)] public Guid StudentId { get; set; }

    [MaxLength(255)] public required string CourseCode { get; set; }

    [MaxLength(255)] public required string CourseId { get; set; }

    public int CourseYearId { get; set; }

    public int CourseSemesterCode { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}