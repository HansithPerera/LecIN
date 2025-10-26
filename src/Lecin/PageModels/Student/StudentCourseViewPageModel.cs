using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using SupabaseShared.Models;
using Client = Supabase.Client;
using Lecin.Pages.Student;

namespace Lecin.PageModels.Student;

[QueryProperty(nameof(Course), "course")]
public partial class StudentCourseViewPageModel(Client client) : BasePageModel
{
    [ObservableProperty] private ObservableCollection<Class>? _classes;
    [ObservableProperty] private Course? _course;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int? _streak;
    [ObservableProperty] private ObservableCollection<CourseStreaksAllTime>? _streaksAllTime;
    [ObservableProperty] private bool _isLeaderboardEmpty;

    [RelayCommand]
    public override async Task LoadDataAsync()
    {
        if (Course == null) return;
        try
        {
            IsLoading = true;
            var filters = new List<IPostgrestQueryFilter>
            {
                new QueryFilter<Class, string>(c => c.CourseCode, Constants.Operator.Equals, Course.Code),
                new QueryFilter<Class, int>(c => c.CourseSemesterCode, Constants.Operator.Equals, Course.SemesterCode),
                new QueryFilter<Class, int>(c => c.CourseYear, Constants.Operator.Equals, Course.Year)
            };
            var classes = await client.From<Class>()
                .Select("*")
                .And(filters)
                .Get();

            var streakFilters = new List<IPostgrestQueryFilter>
            {
                new QueryFilter<CourseStreaksAllTime, string>(cs => cs.CourseCode, Constants.Operator.Equals, Course.Code),
                new QueryFilter<CourseStreaksAllTime, int>(cs => cs.CourseSemesterCode, Constants.Operator.Equals, Course.SemesterCode),
                new QueryFilter<CourseStreaksAllTime, int>(cs => cs.CourseYear, Constants.Operator.Equals, Course.Year),
                new QueryFilter<CourseStreaksAllTime, int>(cs => cs.StreakLength, Constants.Operator.GreaterThan, 0)
            };
            var streak = await client.From<CourseStreaksAllTime>()
                .Select("*")
                .And(streakFilters)
                .Limit(5)
                .Order(cs => cs.StreakLength, Constants.Ordering.Ascending)
                .Get();

            StreaksAllTime = new ObservableCollection<CourseStreaksAllTime>(streak.Models);
            IsLeaderboardEmpty = !StreaksAllTime.Any();

            var ownStreakFilters = new List<IPostgrestQueryFilter>
            {
                new QueryFilter<CourseStreaksAllTime, string>(cs => cs.CourseCode, Constants.Operator.Equals, Course.Code),
                new QueryFilter<CourseStreaksAllTime, int>(cs => cs.CourseSemesterCode, Constants.Operator.Equals, Course.SemesterCode),
                new QueryFilter<CourseStreaksAllTime, int>(cs => cs.CourseYear, Constants.Operator.Equals, Course.Year),
                new QueryFilter<CourseStreaksAllTime, Guid>(cs => cs.StudentId, Constants.Operator.Equals, Guid.Parse(client.Auth.CurrentUser?.Id ?? ""))
            };
            var ownStreak = await client.From<CourseStreaksAllTime>()
                .Select("*")
                .And(ownStreakFilters)
                .Single();

            Streak = ownStreak?.StreakLength;
            Classes = new ObservableCollection<Class>(classes.Models);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading classes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ViewClassmates()
    {
        var code = Course?.Code;
        if (string.IsNullOrWhiteSpace(code)) return;
        await Shell.Current.GoToAsync($"{nameof(ClassmatesPage)}?courseCode={code}");
    }
}
