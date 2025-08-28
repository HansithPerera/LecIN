using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("Courses")]
[PrimaryKey(nameof(Code), nameof(Year), nameof(SemesterCode))]
public class Course
{
    public string Code { get; set; }
    
    public int Year { get; set; }
    
    public int SemesterCode { get; set; }
    
    public string Name { get; set; }
    
    public List<Student> Students { get; set; }
    
    public List<Teacher> Teachers { get; set; }
}