using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using SupabaseShared.Models;
using Client = Supabase.Client;
using Lecin.Pages.Student;
using Lecin.Models;
using Pg = Supabase.Postgrest;
using CourseTeacherModel = Lecin.Models.CourseTeacher;
using TeacherModel = Lecin.Models.Teacher;
using System.Text.Json;

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

    [ObservableProperty] private string _teacherName = string.Empty;
    [ObservableProperty] private string _teacherEmail = string.Empty;

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

            await LoadTeacherAsync(client);
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

    private async Task LoadTeacherAsync(Client client)
    {
        try
        {
            TeacherName = "Not assigned";
            TeacherEmail = "";

            if (Course == null || string.IsNullOrWhiteSpace(Course.Code))
                return;

            var code = Course.Code.Trim();

            var recent = await client
                .From<CourseTeacherModel>()
                .Select("TeacherId,CourseCode,CourseYear,CourseSemesterCode")
                .Filter(nameof(CourseTeacherModel.CourseCode), Pg.Constants.Operator.Equals, code)
                .Order(nameof(CourseTeacherModel.CourseYear), Constants.Ordering.Descending)
                .Order(nameof(CourseTeacherModel.CourseSemesterCode), Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            var map = recent.Models.FirstOrDefault();

            if (map == null || map.TeacherId == Guid.Empty)
            {
                var anyTeacher = await client
                    .From<TeacherModel>()
                    .Select("Id,FirstName,LastName,Email")
                    .Limit(1)
                    .Get();

                var t0 = anyTeacher.Models.FirstOrDefault();
                if (t0 != null)
                {
                    TeacherName = $"{t0.FirstName} {t0.LastName}";
                    TeacherEmail = t0.Email ?? "";
                }
                return;
            }

            var teacher = await client
                .From<TeacherModel>()
                .Select("Id,FirstName,LastName,Email")
                .Filter(nameof(TeacherModel.Id), Pg.Constants.Operator.Equals, map.TeacherId.ToString())
                .Single();

            if (teacher == null)
                return;

            TeacherName = $"{teacher.FirstName} {teacher.LastName}";
            TeacherEmail = teacher.Email ?? "";
        }
        catch
        {
            TeacherName = "Not assigned";
            TeacherEmail = "";
        }
    }

    partial void OnCourseChanged(SupabaseShared.Models.Course? value)
    {
        _ = LoadTeacherAsync(client);
    }


    [RelayCommand]
    private async Task ViewClassmates()
    {
        var code = Course?.Code;
        if (string.IsNullOrWhiteSpace(code)) return;
        await Shell.Current.GoToAsync($"{nameof(ClassmatesPage)}?courseCode={code}");
    }
}
