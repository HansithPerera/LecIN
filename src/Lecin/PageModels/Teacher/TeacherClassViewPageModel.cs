using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using SupabaseShared.Models;
using Client = Supabase.Functions.Client;

namespace Lecin.PageModels.Teacher;

[QueryProperty(nameof(Class), "class")]
public partial class TeacherClassViewPageModel(Supabase.Client client): BasePageModel
{
    [ObservableProperty] 
    private SupabaseShared.Models.Class? _class;
    
    [ObservableProperty]
    private ObservableCollection<Attendance>? _attendances;
    
    [ObservableProperty]
    private ObservableCollection<SupabaseShared.Models.Student>? _students;
    
    [ObservableProperty]
    private string _reason = "";
    
    [ObservableProperty]
    private string _studentId = "";
    
    
    public async Task AddAttendance()
    {
        var studentId = StudentId.Trim();
        var reason = Reason.Trim();
        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(reason)) return;
        if (!Guid.TryParse(studentId, out var studentGuid))
        {
            return;
        }
        if (Class == null) return;
        
        await SetAttendance(studentGuid, reason);
    }
    
    public async Task SetAttendance(Guid studentId, string reason)
    {
        if (Class == null) return;

        await client.Functions.Invoke("teacher-set-attendance", options: new Client.InvokeFunctionOptions()
        {
            Body = new Dictionary<string, object>()
            {
                { "ClassId", Class.Id },
                { "StudentId", studentId },
                { "Reason", reason }
            }
        });
        
        await LoadDataAsync();
        StudentId = "";
        Reason = "";
    }
    
    public override async Task LoadDataAsync()
    {
        try
        {
            if (Class == null) return;

            var courseEqualityFilters = new List<IPostgrestQueryFilter>()
            {
                new QueryFilter("CourseCode", Constants.Operator.Equals, Class.CourseCode),
                new QueryFilter("CourseYear", Constants.Operator.Equals, Class.CourseYear),
                new QueryFilter("CourseSemesterCode", Constants.Operator.Equals, Class.CourseSemesterCode)
            };
            var students = await client.From<Enrollment>()
                .Select("*")
                .And(courseEqualityFilters)
                .Get();
            
            var attendances = await client.From<Attendance>()
                .Select("*")
                .Get();
            
            Attendances = new ObservableCollection<Attendance>(attendances.Models);
            Students = new ObservableCollection<SupabaseShared.Models.Student>(
                students.Models
                    .Select(e => e.Student!)
                    .Where(s => s != null!)
                    .Where(s => Attendances.All(a => a.StudentId != s.Id))
                    .ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load attendances: {ex.Message}");
        }
    }
    
}