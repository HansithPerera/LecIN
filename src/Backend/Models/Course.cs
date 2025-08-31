using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Courses")]
public class Course
{
    public string Code { get; set; }

    public int Year { get; set; }

    public int SemesterCode { get; set; }

    public string Name { get; set; }
}