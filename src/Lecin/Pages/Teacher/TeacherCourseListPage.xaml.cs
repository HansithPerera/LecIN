using Lecin.PageModels.Teacher;
using Supabase;

namespace Lecin.Pages.Teacher;

public partial class TeacherCourseListPage : BaseContentPage
{
    public TeacherCourseListPage(TeacherCourseListPageModel vm, Client client) : base(vm, client)
    {
        InitializeComponent();
    }

    private void GotoCourseView(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: SupabaseShared.Models.Course course }) return;

        try
        {
            Shell.Current.GoToAsync($"teachers/course", new Dictionary<string, object>
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