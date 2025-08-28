using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Classes")]
public class Class
{
    [Key]
    public string Id { get; set; }
    
    public string CourseId { get; set; }
    
    public int CourseYearId { get; set; }
    
    public int CourseSemesterCode { get; set; }
    
    public Course Course { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    
    public TimeSpan Duration => EndTime - StartTime;
    
    public DateTimeOffset EndTime { get; set; }
}