namespace Lecin.Pages.Teacher;

public partial class TeacherDashboardPage : ContentPage
{
    public TeacherDashboardPage()
    {
        InitializeComponent();
    }

    private async void GotoCourseList(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(TeacherCourseListPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to course list: {ex.Message}", "OK");
        }
    }
}