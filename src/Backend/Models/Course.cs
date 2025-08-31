using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("Courses")]
public class Course
{
    public string Code { get; set; }
    
    public int Year { get; set; }
    
    public int SemesterCode { get; set; }
    
    public string Name { get; set; }
}