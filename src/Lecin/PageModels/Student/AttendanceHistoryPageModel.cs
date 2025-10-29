using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Supabase.Postgrest;
using SupabaseShared.Models;
using Lecin.PageModels;
using Color = Microsoft.Maui.Graphics.Color;
using Client = Supabase.Client;

namespace Lecin.PageModels.Student;

public partial class AttendanceHistoryPageModel(Client client) : BasePageModel
{
    public ObservableCollection<AttendanceRecord> Records { get; } = new();

    public override async Task LoadDataAsync()
    {
        Records.Clear();
        if (!Guid.TryParse(client.Auth.CurrentUser?.Id, out var userId)) return;

        try
        {
            var to = DateTimeOffset.Now;
            var from = to.AddMonths(-6);

            // 1) Load all attendance for current user in range
            var attendanceResp = await client
                .From<Attendance>()
                .Select("*")
                .Where(a => a.StudentId == userId)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to)
                .Get();

            var attendanceByClass = attendanceResp.Models.ToDictionary(a => a.ClassId, a => a);

            // Preload classes in range for enrolled courses to enrich with CourseCode
            var classMap = new Dictionary<Guid, Class>();
            var enrollments = await client
                .From<Enrollment>()
                .Select("*")
                .Where(e => e.StudentId == userId)
                .Get();

            foreach (var e in enrollments.Models)
            {
                var classesResp = await client
                    .From<Class>()
                    .Select("*")
                    .Where(c => c.CourseCode == e.CourseCode)
                    .Where(c => c.CourseYear == e.CourseYear)
                    .Where(c => c.CourseSemesterCode == e.CourseSemesterCode)
                    .Where(c => c.StartTime >= from && c.StartTime <= to)
                    .Get();
                foreach (var c in classesResp.Models)
                    if (!classMap.ContainsKey(c.Id)) classMap[c.Id] = c;
            }

            foreach (var a in attendanceResp.Models)
            {
                var localTs = a.Timestamp.ToLocalTime();
                classMap.TryGetValue(a.ClassId, out var clazz);
                Records.Add(new AttendanceRecord
                {
                    Date = localTs.ToString("yyyy-MM-dd HH:mm"),
                    Status = MapStatus(a.Reason),
                    CourseCode = clazz?.CourseCode,
                    SortTimestamp = localTs
                });
            }

            // 2) Add Absent rows for classes without attendance

            foreach (var e in enrollments.Models)
            {
                var classesResp = await client
                    .From<Class>()
                    .Select("*")
                    .Where(c => c.CourseCode == e.CourseCode)
                    .Where(c => c.CourseYear == e.CourseYear)
                    .Where(c => c.CourseSemesterCode == e.CourseSemesterCode)
                    .Where(c => c.StartTime >= from && c.StartTime <= to)
                    .Get();

                foreach (var c in classesResp.Models)
                {
                    if (!attendanceByClass.ContainsKey(c.Id))
                    {
                        var localTs = c.StartTime.ToLocalTime();
                        Records.Add(new AttendanceRecord
                        {
                            Date = localTs.ToString("yyyy-MM-dd HH:mm"),
                            Status = "Absent",
                            CourseCode = c.CourseCode,
                            SortTimestamp = localTs
                        });
                    }
                }
            }

            // 3) Sort descending by timestamp
            var sorted = Records.OrderByDescending(r => r.SortTimestamp).ToList();
            Records.Clear();
            foreach (var r in sorted) Records.Add(r);
        }
        catch
        {
            // leave empty on error
        }
    }

    private static string MapStatus(string? reason)
        => string.IsNullOrWhiteSpace(reason) ? "Present" : reason;
}

public class AttendanceRecord : ObservableObject
{
    public required string Date { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset SortTimestamp { get; set; }
    public string? CourseCode { get; set; }

    public string StatusText => string.IsNullOrWhiteSpace(CourseCode)
        ? $"{Date} - {Status}"
        : $"{Date} · {CourseCode} · {Status}";
    public Color StatusColor => Status switch
    {
        "Late" => Colors.Orange,
        "Excused" => Colors.DodgerBlue,
        "Absent" => Colors.Red,
        _ => Colors.Green
    };
}
