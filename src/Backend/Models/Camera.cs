using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Cameras")]
public class Camera
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)] public required string Name { get; set; }

    [MaxLength(2048)] public required string Location { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; }
}