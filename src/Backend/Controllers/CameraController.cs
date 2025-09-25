using System.Security.Claims;
using Backend.Database;
using Backend.Dto;
using Backend.Face;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupabaseShared.Models;
using System.Text.Json;

namespace Backend.Controllers;

public class AttendanceRecord
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string LectureId { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

[Route("api/[controller]")]
[Authorize(Policy = Constants.CameraAuthorizationPolicy)]
[ApiController]
public class CameraController(Repository service, CameraService cameraService, FaceService faceService, ILogger<CameraController> logger) : ControllerBase
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(new { 
            Message = "Camera API is running", 
            Time = DateTime.UtcNow,
            FaceServiceAvailable = true 
        });
    }

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

    [HttpPost("register-student")]
    public async Task<IActionResult> RegisterStudent([FromForm] List<IFormFile> photos, [FromForm] string studentId, [FromForm] string studentName)
    {
        if (photos == null || !photos.Any())
            return BadRequest("Photos are required");

        try
        {
            if (!Guid.TryParse(studentId, out var studentGuid))
                return BadRequest("Student ID must be a valid GUID");

            foreach (var photo in photos)
            {
                using var stream = photo.OpenReadStream();
                await faceService.TrainOnFaceAsync(studentGuid, stream);
            }

            return Ok(new { Success = true, Message = $"Student {studentName} registered with {photos.Count} photos" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("mark-attendance")]
    public async Task<IActionResult> MarkAttendance([FromForm] IFormFile image, [FromForm] string lectureId)
    {
        if (image == null || string.IsNullOrEmpty(lectureId))
            return BadRequest("Image and lecture ID are required");

        try
        {
            using var stream = image.OpenReadStream();
            var recognizedFace = await faceService.RecognizeFaceAsync(stream, threshold: 80);

            if (recognizedFace != null)
            {
                var record = new AttendanceRecord
                {
                    StudentId = recognizedFace.PersonId.ToString(),
                    StudentName = "Student",
                    CheckInTime = DateTime.UtcNow,
                    LectureId = lectureId,
                    Confidence = 0.8
                };

                await SaveAttendanceAsync(record);

                return Ok(new
                {
                    Success = true,
                    Message = "Student marked present",
                    StudentId = recognizedFace.PersonId,
                    LectureId = lectureId
                });
            }
            else
            {
                return Ok(new
                {
                    Success = false,
                    Message = "No recognized student found in image"
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    private async Task SaveAttendanceAsync(AttendanceRecord record)
    {
        var attendanceDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Attendance");
        Directory.CreateDirectory(attendanceDir);

        var attendanceFile = Path.Combine(attendanceDir, "attendance_records.json");
        var records = new List<AttendanceRecord>();

        if (System.IO.File.Exists(attendanceFile))
        {
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(attendanceFile);
                records = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new();
            }
            catch
            {
                records = new List<AttendanceRecord>();
            }
        }

        var existingRecord = records.FirstOrDefault(r =>
            r.StudentId == record.StudentId &&
            r.LectureId == record.LectureId);

        if (existingRecord == null)
        {
            records.Add(record);
            await System.IO.File.WriteAllTextAsync(attendanceFile, JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [HttpGet("attendance/{lectureId}")]
    public async Task<IActionResult> GetAttendance(string lectureId)
    {
        var attendanceFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Attendance", "attendance_records.json");

        if (!System.IO.File.Exists(attendanceFile))
        {
            return Ok(new { Students = new List<AttendanceRecord>(), Count = 0 });
        }

        var json = await System.IO.File.ReadAllTextAsync(attendanceFile);
        var allRecords = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new();

        var lectureAttendance = allRecords.Where(r => r.LectureId == lectureId).ToList();

        return Ok(new
        {
            Students = lectureAttendance,
            Count = lectureAttendance.Count,
            LectureId = lectureId
        });
    }
}