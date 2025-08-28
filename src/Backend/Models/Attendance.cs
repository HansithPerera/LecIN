using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Attendance")]
public class Attendance
{
    [ForeignKey(nameof(Student))]
    public string StudentId { get; set; }
    
    public Student Student { get; set; }
    
    [ForeignKey(nameof(Class))]
    public string ClassId { get; set; }
    
    public Class Class { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}