using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("Cameras")]
public class Camera
{
    [Column("Id")] public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)] [Column("Name")] public required string Name { get; set; }

    [MaxLength(2048)] [Column("Location")] public required string Location { get; set; }

    [Column("IsActive")] public bool IsActive { get; set; }

    [Column("CreatedAt")] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("UpdatedAt")] public DateTimeOffset UpdatedAt { get; set; }

    public List<CameraApiKey>? CameraApiKeys { get; set; }
}