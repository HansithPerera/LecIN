using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Cameras")]
public class Camera
{
    [Column("Id")] public required Guid Id { get; set; } = Guid.NewGuid();

    [Column("Location")] public required string Location { get; set; }
    
    [Column("IsActive")] public bool IsActive { get; set; }
    
    [Column("CreatedAt")] public DateTimeOffset CreatedAt { get; set; }
    
    [Column("UpdatedAt")] public DateTimeOffset UpdatedAt { get; set; }
    
    public List<CameraApiKey>? CameraApiKeys { get; set; }
}