using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Supabase;
using SupabaseShared.Models;
using Lecin.Services;

namespace Lecin.PageModels.Teacher;

public partial class TeacherCourseListPageModel(Client client, AuthService authService) : BasePageModel
{
    [ObservableProperty] private ObservableCollection<Course> _courses = [];

    public override async Task LoadDataAsync()
    {
        try
        {
            var teacherId = authService.CurrentUserId;
            if (teacherId == null)
            {
                Courses = new ObservableCollection<Course>();
                return;
            }

            // Query courses that this teacher teaches via CourseTeachers junction table
            var courseTeachers = await client
                .From<CourseTeacher>()
                .Where(ct => ct.TeacherId == teacherId.Value)
                .Get();

            if (courseTeachers.Models.Count == 0)
            {
                Courses = new ObservableCollection<Course>();
                return;
            }

            // Get the unique courses
            var uniqueCourses = courseTeachers.Models
                .Select(ct => new { ct.CourseCode, ct.CourseYear, ct.CourseSemesterCode })
                .Distinct()
                .ToList();

            // Fetch full course details
            var courses = new List<Course>();
            foreach (var ct in uniqueCourses)
            {
                var result = await client
                    .From<Course>()
                    .Where(c => c.Code == ct.CourseCode)
                    .Where(c => c.Year == ct.CourseYear)
                    .Where(c => c.SemesterCode == ct.CourseSemesterCode)
                    .Get();

                if (result.Models.Count > 0)
                {
                    courses.Add(result.Models[0]);
                }
            }

            Courses = new ObservableCollection<Course>(courses);
        }
        catch (Exception ex)
        {
            // If there's an error (e.g., CourseTeachers table doesn't exist), fall back to showing all courses
            System.Diagnostics.Debug.WriteLine($"Error loading teacher courses: {ex.Message}");
            try
            {
                var courses = await client.From<Course>().Select("*").Get();
                Courses = new ObservableCollection<Course>(courses.Models);
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback also failed: {fallbackEx.Message}");
                Courses = new ObservableCollection<Course>();
            }
        }
    }
}