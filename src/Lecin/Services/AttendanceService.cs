using Lecin.Models;
using SupabaseShared.Models;

namespace Lecin.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceStats> GetStudentAttendanceStatsAsync(Guid studentId);
        Task<List<AttendanceStats>> GetAllStudentsAttendanceStatsAsync();
        Task<List<Student>> GetAllStudentsAsync();
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly Supabase.Client _supabaseClient;

        public AttendanceService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<AttendanceStats> GetStudentAttendanceStatsAsync(Guid studentId)
        {
            try
            {
                // Step 1: Get all classes the student is enrolled in
                var enrollments = await _supabaseClient
                    .From<Enrollment>()
                    .Where(e => e.StudentId == studentId)
                    .Get();

                var totalClassesEnrolled = enrollments.Models.Count;

                // Step 2: Get all attendance records for this student
                var attendanceRecords = await _supabaseClient
                    .From<Attendance>()
                    .Where(a => a.StudentId == studentId)
                    .Get();

                // Step 3: Count unique classes attended
                // (In case a student has multiple attendance records for the same class)
                var uniqueClassesAttended = attendanceRecords.Models
                    .Select(a => a.ClassId)
                    .Distinct()
                    .Count();

                // Step 4: Create and return the attendance stats
                return new AttendanceStats
                {
                    StudentId = studentId,
                    TotalClassesEnrolled = totalClassesEnrolled,
                    ClassesAttended = uniqueClassesAttended
                };
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your preferred logging)
                System.Diagnostics.Debug.WriteLine($"Error getting attendance stats for {studentId}: {ex.Message}");
                
                // Return empty stats on error
                return new AttendanceStats
                {
                    StudentId = studentId,
                    TotalClassesEnrolled = 0,
                    ClassesAttended = 0
                };
            }
        }

        public async Task<List<AttendanceStats>> GetAllStudentsAttendanceStatsAsync()
        {
            try
            {
                // Get all students
                var students = await _supabaseClient
                    .From<Student>()
                    .Get();

                var allStats = new List<AttendanceStats>();

                // Get attendance stats for each student
                foreach (var student in students.Models)
                {
                    var stats = await GetStudentAttendanceStatsAsync(student.Id);
                    allStats.Add(stats);
                }

                return allStats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all students attendance: {ex.Message}");
                return new List<AttendanceStats>();
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            try
            {
                var result = await _supabaseClient
                    .From<Student>()
                    .Get();

                return result.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting students: {ex.Message}");
                return new List<Student>();
            }
        }

        // Bonus: Get attendance stats for a specific date range
        public async Task<AttendanceStats> GetStudentAttendanceStatsByDateAsync(
            Guid studentId, 
            DateTime fromDate, 
            DateTime toDate)
        {
            try
            {
                // Get enrollments (total classes remain the same)
                var enrollments = await _supabaseClient
                    .From<Enrollment>()
                    .Where(e => e.StudentId == studentId)
                    .Get();

                var totalClassesEnrolled = enrollments.Models.Count;

                // Get attendance records within the date range
                var attendanceRecords = await _supabaseClient
                    .From<Attendance>()
                    .Where(a => a.StudentId == studentId)
                    .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                    .Get();

                var uniqueClassesAttended = attendanceRecords.Models
                    .Select(a => a.ClassId)
                    .Distinct()
                    .Count();

                return new AttendanceStats
                {
                    StudentId = studentId,
                    TotalClassesEnrolled = totalClassesEnrolled,
                    ClassesAttended = uniqueClassesAttended
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting attendance stats by date for {studentId}: {ex.Message}");
                return new AttendanceStats
                {
                    StudentId = studentId,
                    TotalClassesEnrolled = 0,
                    ClassesAttended = 0
                };
            }
        }
    }
}