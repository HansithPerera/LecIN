using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Postgrest;
using SupabaseShared.Models;
using Client = Supabase.Client;
using Exception = System.Exception;

namespace Lecin.PageModels.Student;

public partial class StudentDashboardPageModel(Client client) : BasePageModel
{
    [ObservableProperty] private List<Course> _courses = new();

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private SupabaseShared.Models.Student? _student;

    [ObservableProperty] private int? _totalStreak;

    [ObservableProperty] private List<Class> _upcomingClasses = new();

    [ObservableProperty] private string? _welcomeMessage;

    [RelayCommand]
    public override async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            if (!Guid.TryParse(client.Auth.CurrentUser?.Id, out var userId)) return;
            var studentTask = client.From<SupabaseShared.Models.Student>()
                .Where(s => s.Id == userId)
                .Single();

            var enrollmentsTask = client.From<Enrollment>()
                .Select("*")
                .Where(e => e.StudentId == userId)
                .Get();

            var upcomingClassesTask = client.From<Class>()
                .Select("*")
                .Order(nameof(Class.StartTime), Constants.Ordering.Ascending)
                .Where(e => e.StartTime >= DateTimeOffset.Now)
                .Limit(5)
                .Get();

            var totalStreakTask = client.Postgrest.Rpc<int>("CalculateStudentStreak");

            await Task.WhenAll(studentTask, enrollmentsTask, upcomingClassesTask, totalStreakTask);

            var enrollmentsResponse = enrollmentsTask.Result;
            var upcomingClassesResponse = upcomingClassesTask.Result;

            UpcomingClasses = upcomingClassesResponse.Models;
            Courses = enrollmentsResponse.Models
                .Select(enrollment => enrollment.Course)
                .Where(course => course != null)
                .ToList()!;

            Student = studentTask.Result;
            TotalStreak = totalStreakTask.Result;
            WelcomeMessage = $"Welcome, {Student?.FirstName}!";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
}