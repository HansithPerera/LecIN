using Backend.Face;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CameraController(FaceService faceService): ControllerBase
{
    [HttpPost("face")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecognizeFace([FromForm] IFormFile file)
    {
        throw new NotImplementedException();
    }
}