using Lecin.Pages.Teacher;
using Supabase;
using SupabaseShared.Models;
using StudentCourseViewPageModel = Lecin.PageModels.Student.StudentCourseViewPageModel;

namespace Lecin.Pages.Student;

public partial class StudentCourseViewPage
{
    public StudentCourseViewPage(StudentCourseViewPageModel vm, Client client) : base(vm)
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
}