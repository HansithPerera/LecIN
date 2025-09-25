using Microsoft.AspNetCore.Mvc;
using AttendanceCheckInBackend.Models;
using AttendanceCheckInBackend.Services;

namespace AttendanceCheckInBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly FacialRecognitionService _recognitionService = new FacialRecognitionService();
        private static List<Student> _students = new List<Student>
        {
            new Student { Id = 1, Name = "Alice", StudentId = "S123", FaceEncoding = "face123" },
            new Student { Id = 2, Name = "Bob", StudentId = "S124", FaceEncoding = "face124" }
        };

        private static List<AttendanceRecord> _attendanceRecords = new List<AttendanceRecord>();

        [HttpPost]
        public IActionResult CheckIn([FromBody] string faceEncoding)
        {
            var student = _recognitionService.RecognizeFace(faceEncoding, _students);
            if (student != null)
            {
                var record = new AttendanceRecord
                {
                    Id = _attendanceRecords.Count + 1,
                    StudentId = student.Id,
                    Timestamp = DateTime.Now,
                    IsPresent = true
                };
                _attendanceRecords.Add(record);
                return Ok(new { message = "✅ Attendance recorded", student.Name, record.Timestamp });
            }
            else
            {
                return NotFound(new
                {
                    message = "❌ Face not recognized",
                    alternatives = new[] { "Manual Entry", "Scan Student ID" }
                });
            }
        }
    }
}