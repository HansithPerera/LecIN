using Lecin.PageModels.Teacher;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.Pages.Teacher;

public partial class TeacherCourseListPage : BaseContentPage
{
    public TeacherCourseListPage(TeacherCourseListPageModel vm) : base(vm)
    {
        InitializeComponent();
    }

    private void GotoCourseView(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Course course }) return;

        try
        {
            Shell.Current.GoToAsync(nameof(TeacherCourseViewPage), new Dictionary<string, object>
            {
                { "course", course }
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to navigate to course view: {ex.Message}", "OK");
        }
    }
}