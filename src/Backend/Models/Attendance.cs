using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("Attendance")]
[PrimaryKey(nameof(StudentId), nameof(ClassId))]
public class Attendance
{
    [MaxLength(255)] public Guid StudentId { get; set; }

    [MaxLength(255)] public Guid ClassId { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}