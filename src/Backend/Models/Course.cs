using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Course
{
    [Key]
    public string Code { get; set; }
    
    public string Name { get; set; }
    
    public List<Student> Students { get; set; }
    
    public List<Teacher> Teachers { get; set; }
}