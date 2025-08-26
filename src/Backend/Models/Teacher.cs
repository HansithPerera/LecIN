using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Teacher
{
    
    [Key]
    public string Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    
}