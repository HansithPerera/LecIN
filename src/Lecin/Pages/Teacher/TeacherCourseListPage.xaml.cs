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

    private async void GotoCourseView(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button { BindingContext: Course course }) return;
            await Shell.Current.GoToAsync(nameof(TeacherCourseViewPage), new Dictionary<string, object>
            {
                { "course", course }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to course view: {ex.Message}", "OK");
        }
    }
}