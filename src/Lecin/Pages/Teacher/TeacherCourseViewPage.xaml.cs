using Lecin.PageModels.Teacher;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.Pages.Teacher;

public partial class TeacherCourseViewPage
{
    public TeacherCourseViewPage(TeacherCourseViewPageModel vm, Client client) : base(vm, client)
    {
        InitializeComponent();
    }

    private async void GotoClassView(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button { BindingContext: Class cls }) return;

            await Shell.Current.GoToAsync("teacher/class", new Dictionary<string, object>
            {
                { "class", cls }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to class view: {ex.Message}", "OK");
        }
    }
}