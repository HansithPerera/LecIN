using Backend.Models;

namespace Backend.Dto.Resp;

public class CameraDto
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Location { get; set; }

    public required bool IsActive { get; set; }

    public static CameraDto FromModel(Camera camera)
    {
        return new CameraDto
        {
            Id = camera.Id,
            Name = camera.Name,
            Location = camera.Location,
            IsActive = camera.IsActive
        };
    }
}