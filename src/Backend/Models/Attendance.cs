using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Attendance")]
public class Attendance
{
    [ForeignKey(nameof(Student))]
    [MaxLength(255)]
    public required string StudentId { get; set; }

    public required Student Student { get; set; }

    [ForeignKey(nameof(Class))]
    [MaxLength(255)]
    public required string ClassId { get; set; }

    public required Class Class { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}