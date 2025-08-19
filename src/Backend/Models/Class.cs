namespace Backend.Models;

public class Class
{
    public string Code { get; set; }
    public string Name { get; set; }
    
    public List<Student> Students { get; set; }
    
    public List<Teacher> Teachers { get; set; }
}