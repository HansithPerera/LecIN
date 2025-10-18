using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SupabaseShared.Models;

namespace Lecin.PageModels.Teacher;

[QueryProperty(nameof(Course), "course")]
public partial class TeacherCourseViewPageModel(Supabase.Client client): BasePageModel
{
    [ObservableProperty] private Course? _course;
    
    [ObservableProperty] private ObservableCollection<Class>? _classes;


    public override async Task LoadDataAsync()
    {
        if (Course == null) return;

        try
        {
            var classes = await client.From<Class>()
                .Where(c => c.CourseCode == Course.Code && c.CourseSemesterCode == Course.SemesterCode && c.CourseYear == Course.Year)
                .Select(c => new object[]{ "*", c.Attendance})
                .Get();

            Classes = new ObservableCollection<Class>(classes.Models);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading classes: {ex.Message}");
        }
    }
}