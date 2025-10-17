using CommunityToolkit.Mvvm.ComponentModel;
using Supabase.Postgrest;
using Exception = System.Exception;

namespace Lecin.PageModels.Student;

public partial class StudentDashboardPageModel(Supabase.Client client) : BasePageModel
{
    
    [ObservableProperty]
    private bool _loaded = false;

    [ObservableProperty] 
    private string? _welcomeMessage;
    
    [ObservableProperty]
    private SupabaseShared.Models.Student? _student;
    
    [ObservableProperty]
    private List<SupabaseShared.Models.Course> _courses = new();
    
    [ObservableProperty]
    private List<SupabaseShared.Models.Class> _upcomingClasses = new();
    
    public override async Task LoadDataAsync()
    {
        try
        {
            if (!Guid.TryParse(client.Auth.CurrentUser?.Id, out var userId))
            {
                return;
            }
            var studentTask = client.From<SupabaseShared.Models.Student>()
                .Where(s => s.Id == userId)
                .Single();

            var enrollmentsTask = client.From<SupabaseShared.Models.Enrollment>()
                .Select("*")
                .Where(e => e.StudentId == userId)
                .Get();
            
            var upcomingClassesTask = client.From<SupabaseShared.Models.Class>()
                .Select("*")
                .Order(nameof(SupabaseShared.Models.Class.StartTime), Constants.Ordering.Ascending)
                .Where(e => e.StartTime >= DateTimeOffset.Now)
                .Limit(5)
                .Get();
            
            await Task.WhenAll(studentTask, enrollmentsTask, upcomingClassesTask);
            var studentResponse = studentTask.Result;
            var enrollmentsResponse = enrollmentsTask.Result;
            var upcomingClassesResponse = upcomingClassesTask.Result;
            
            if (studentResponse == null)
            {
                return;
            }
            
            UpcomingClasses = upcomingClassesResponse.Models;
            Courses = enrollmentsResponse.Models
                .Select(enrollment => enrollment.Course)
                .Where(course => course != null)
                .ToList()!;
            
            Student = studentResponse;
            WelcomeMessage = $"Welcome, {Student.FirstName}!";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            Loaded = true;
        }
    }
}