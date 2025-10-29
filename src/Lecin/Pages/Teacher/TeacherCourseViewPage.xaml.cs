using Lecin.PageModels.Teacher;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.Pages.Teacher;

public partial class TeacherCourseViewPage
{
    public TeacherCourseViewPage(TeacherCourseViewPageModel vm, Client client) : base(vm)
    {
        InitializeComponent();
    }

    private async void GotoClassView(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button { BindingContext: Class cls }) return;

            await Shell.Current.GoToAsync(nameof(TeacherClassViewPage), new Dictionary<string, object>
            {
                { "class", cls }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to class view: {ex.Message}", "OK");
        }
    }

    private async void OnAttendanceReportsClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TeacherCourseViewPageModel vm) return;
            if (vm.Course == null) return;

            await Shell.Current.GoToAsync($"{nameof(TeacherReportsPage)}?courseCode={vm.Course.Code}&courseName={Uri.EscapeDataString(vm.Course.Name)}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to attendance reports: {ex.Message}", "OK");
        }
    }
}