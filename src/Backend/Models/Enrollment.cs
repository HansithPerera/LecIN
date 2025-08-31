using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Enrollments")]
public class Enrollment
{
    [ForeignKey(nameof(Student))] public string StudentId { get; set; }

    public Student Student { get; set; }

    public string CourseCode { get; set; }

    public string CourseId { get; set; }

    public int CourseYearId { get; set; }

    public int CourseSemesterCode { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}