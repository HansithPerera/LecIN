using SupabaseShared.Models;
using Lecin.Models;

namespace Lecin.Services
{
    public interface IAttendanceHistoryService
    {
        Task<List<AttendanceHistoryItem>> GetStudentAttendanceHistoryAsync(string studentId);
        Task<AttendanceDetailItem?> GetAttendanceDetailAsync(string studentId, string classId, DateTime date);
        Task<List<AttendanceHistoryItem>> GetTestAttendanceHistoryAsync();
    }

    public class AttendanceHistoryService : IAttendanceHistoryService
    {
        private readonly Supabase.Client _supabaseClient;

        public AttendanceHistoryService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<AttendanceHistoryItem>> GetStudentAttendanceHistoryAsync(string studentId)
        {
            try
            {
                // Convert string studentId to Guid
                if (!Guid.TryParse(studentId, out Guid studentGuid))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid student ID format: {studentId}");
                    return new List<AttendanceHistoryItem>();
                }

                // Get all attendance records for the student
                var attendanceRecords = await _supabaseClient
                    .From<SupabaseShared.Models.Attendance>()
                    .Where(a => a.StudentId == studentGuid)
                    .Order(a => a.Timestamp, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                var historyItems = new List<AttendanceHistoryItem>();

                foreach (var record in attendanceRecords.Models)
                {
                    var historyItem = new AttendanceHistoryItem
                    {
                        StudentId = record.StudentId.ToString(),
                        ClassId = record.ClassId.ToString(),
                        Date = record.Timestamp.Date,
                        Time = record.Timestamp.ToString("HH:mm"),
                        Status = DetermineAttendanceStatus(record.Reason),
                        RawReason = record.Reason ?? "Present"
                    };

                    historyItems.Add(historyItem);
                }

                return historyItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting attendance history for {studentId}: {ex.Message}");
                return new List<AttendanceHistoryItem>();
            }
        }

        public async Task<AttendanceDetailItem?> GetAttendanceDetailAsync(string studentId, string classId, DateTime date)
        {
            try
            {
                // Convert string IDs to Guids
                if (!Guid.TryParse(studentId, out Guid studentGuid) || !Guid.TryParse(classId, out Guid classGuid))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid ID format - StudentId: {studentId}, ClassId: {classId}");
                    return null;
                }

                // Get the specific attendance record
                var attendanceRecord = await _supabaseClient
                    .From<SupabaseShared.Models.Attendance>()
                    .Where(a => a.StudentId == studentGuid)
                    .Where(a => a.ClassId == classGuid)
                    .Where(a => a.Timestamp.Date == date.Date)
                    .Get();

                if (!attendanceRecord.Models.Any())
                    return null;

                var record = attendanceRecord.Models.First();

                // Get class information - use the correct property name
                var classInfo = await _supabaseClient
                    .From<SupabaseShared.Models.Class>()
                    .Where(c => c.Id == classGuid)  // Use Id instead of ClassId
                    .Get();

                var detailItem = new AttendanceDetailItem
                {
                    StudentId = record.StudentId.ToString(),
                    ClassId = record.ClassId.ToString(),
                    Date = record.Timestamp.Date,
                    Time = record.Timestamp.ToString("HH:mm"),
                    Status = DetermineAttendanceStatus(record.Reason),
                    CourseName = classInfo.Models.FirstOrDefault()?.CourseCode ?? "Unknown Course",
                    ClassTime = record.Timestamp.ToString("HH:mm"),
                    AttendanceType = DetermineAttendanceStatus(record.Reason),
                    Notes = record.Reason ?? "No notes"
                };

                return detailItem;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting attendance detail: {ex.Message}");
                return null;
            }
        }

        private AttendanceStatus DetermineAttendanceStatus(string? reason)
        {
            if (string.IsNullOrEmpty(reason))
                return AttendanceStatus.Present;

            var lowerReason = reason.ToLower().Trim();

            if (lowerReason.Contains("present") || lowerReason == "present" || string.IsNullOrEmpty(reason))
                return AttendanceStatus.Present;
            else if (lowerReason.Contains("late"))
                return AttendanceStatus.Late;
            else if (lowerReason.Contains("excused") || lowerReason.Contains("excuse"))
                return AttendanceStatus.Excused;
            else if (lowerReason.Contains("absent"))
                return AttendanceStatus.Absent;
            else
                return AttendanceStatus.Present; // Default to present if unclear
        }

        // Helper method to create test data for demonstration
        public async Task<List<AttendanceHistoryItem>> GetTestAttendanceHistoryAsync()
        {
            // Create some test data to demonstrate the feature
            var testData = new List<AttendanceHistoryItem>
            {
                new AttendanceHistoryItem
                {
                    StudentId = "test-student-1",
                    ClassId = "test-class-1",
                    Date = DateTime.Now.AddDays(-1),
                    Time = "09:00",
                    Status = AttendanceStatus.Present,
                    RawReason = "Present"
                },
                new AttendanceHistoryItem
                {
                    StudentId = "test-student-1",
                    ClassId = "test-class-2",
                    Date = DateTime.Now.AddDays(-2),
                    Time = "14:00",
                    Status = AttendanceStatus.Late,
                    RawReason = "Late - Traffic"
                },
                new AttendanceHistoryItem
                {
                    StudentId = "test-student-1",
                    ClassId = "test-class-3",
                    Date = DateTime.Now.AddDays(-3),
                    Time = "11:00",
                    Status = AttendanceStatus.Absent,
                    RawReason = "Absent - Sick"
                },
                new AttendanceHistoryItem
                {
                    StudentId = "test-student-1",
                    ClassId = "test-class-4",
                    Date = DateTime.Now.AddDays(-4),
                    Time = "10:00",
                    Status = AttendanceStatus.Excused,
                    RawReason = "Excused - Medical appointment"
                },
                new AttendanceHistoryItem
                {
                    StudentId = "test-student-1",
                    ClassId = "test-class-5",
                    Date = DateTime.Now.AddDays(-5),
                    Time = "13:00",
                    Status = AttendanceStatus.Present,
                    RawReason = "Present"
                }
            };

            return testData;
        }
    }
}