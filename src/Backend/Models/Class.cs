using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Class
{
    
    [Key]
    public string Id { get; set; }
    
    [ForeignKey(nameof(Course))]
    public string CourseId { get; set; }
    
    public Course Course { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    
    public DateTimeOffset EndTime { get; set; }
    
    public string Location { get; set; }
}