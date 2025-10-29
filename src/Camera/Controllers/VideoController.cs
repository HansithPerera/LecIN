using Camera.Services;
using Microsoft.AspNetCore.Mvc;

namespace Camera.Controllers;

[ApiController]
public class VideoController(FrameBuffer frameBuffer) : Controller
{
    [HttpGet("latest-frame")]
    public IActionResult GetLatestFrame()
    {
        frameBuffer.GetFrame(out var frame);
        if (frame == null || frame.Length == 0)
            return NotFound("No frame available.");
        return File(frame, "image/jpeg");
    }
}