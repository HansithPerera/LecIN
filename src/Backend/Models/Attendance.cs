using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Attendance")]
public class Attendance
{
    [MaxLength(255)] public required string StudentId { get; set; }

    public required Student Student { get; set; }

    [MaxLength(255)] public required string ClassId { get; set; }

    public required Class Class { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}