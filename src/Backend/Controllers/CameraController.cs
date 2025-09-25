using System.Security.Claims;
using Backend.Database;
using Backend.Dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupabaseShared.Models;

namespace Backend.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = Constants.CameraAuthorizationPolicy)]
[ApiController]
public class CameraController(Repository service, CameraService cameraService, ILogger<CameraController> logger) : ControllerBase
{
    [HttpGet("details")]
    [ProducesResponseType(typeof(Camera), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCameraDetails()
    {
        if (!Guid.TryParse(HttpContext.User.Identity?.Name, out var apiKeyId)) return Unauthorized();
        var camera = await service.GetCameraByApiKeyIdAsync(apiKeyId);
        return camera == null
            ? NotFound("Camera not found.")
            : Ok(CameraDto.FromModel(camera));
    }

    [HttpPost("mark-attendance")]
    public async Task<IActionResult> MarkAttendance([FromForm] IFormFile image, [FromForm] string lectureId)
    {
        if (!Guid.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var apiKeyId))
            return Unauthorized();

        if (image == null || string.IsNullOrEmpty(lectureId))
            return BadRequest("Image and lecture ID are required");

        try
        {
            await using var stream = image.OpenReadStream();
            var faceDetectionResult = await cameraService.CheckFaceIntoClass(apiKeyId, stream);
            
            if (faceDetectionResult.IsErr)
            {
                logger.LogWarning("Face recognition failed: {Error}", faceDetectionResult.UnwrapErr());
                return BadRequest(new { 
                    Success = false, 
                    Error = faceDetectionResult.UnwrapErr(),
                    Message = "Face recognition failed" 
                });
            }
            
            var attendance = faceDetectionResult.Unwrap();
            logger.LogInformation("Student {StudentId} marked present for class {ClassId}", 
                attendance.StudentId, attendance.ClassId);
                
            return Ok(new { 
                Success = true, 
                StudentId = attendance.StudentId, 
                ClassId = attendance.ClassId,
                LectureId = lectureId,
                Message = "Attendance marked successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking attendance");
            return StatusCode(500, new { 
                Success = false,
                Error = "Internal server error occurred while marking attendance" 
            });
        }
    }
}