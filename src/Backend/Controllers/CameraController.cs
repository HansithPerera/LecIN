using Backend.Database;
using Backend.Dto.Resp;
using Backend.Face;
using Backend.Models;
using Backend.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = Constants.CameraAuthorizationPolicy, AuthenticationSchemes = Constants.ApiKeyAuthScheme)]
[ApiController]
public class CameraController(AppService service, FaceService faceService) : ControllerBase
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

    [HttpPost("face")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecognizeFace([FromForm] IFormFile file)
    {
        if (!Guid.TryParse(HttpContext.User.Identity?.Name, out var apiKeyId)) return Unauthorized();

        var classroomResult = await CameraRules.GetOngoingClass(apiKeyId, service);
        if (classroomResult.IsErr)
            return classroomResult.UnwrapErr() switch
            {
                Errors.ClassRetrievalError.NoCameraFound =>
                    Unauthorized(),
                Errors.ClassRetrievalError.NoClassFound =>
                    NotFound("No class found at this location and time."),
                _ => StatusCode(500, "An unknown error occurred.")
            };

        var classroom = classroomResult.Unwrap();

        await using var stream = file.OpenReadStream();
        var checkinResult = await CameraRules.CheckFaceIntoClass(classroom.Id, stream, service, faceService);

        if (checkinResult.IsErr)
            return checkinResult.UnwrapErr() switch
            {
                Errors.CheckInError.ClassNotFound =>
                    NotFound("Class not found."),
                Errors.CheckInError.StudentNotFound =>
                    NotFound("Student not found."),
                Errors.CheckInError.StudentNotEnrolled =>
                    BadRequest("Student is not enrolled in this class."),
                Errors.CheckInError.FaceRecognitionFailed =>
                    BadRequest("Face recognition failed. Ensure the image is clear and contains a single face."),
                _ => StatusCode(500, "An unknown error occurred.")
            };

        return Ok();
    }
}