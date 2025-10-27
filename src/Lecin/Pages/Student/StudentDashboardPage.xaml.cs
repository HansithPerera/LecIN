using Lecin.PageModels.Student;
using Microsoft.Extensions.DependencyInjection;
using SupabaseShared.Models;

namespace Lecin.Pages.Student;

public partial class StudentDashboardPage : BaseContentPage
{
    public StudentDashboardPage(StudentDashboardPageModel vm) : base(vm)
    {
        InitializeComponent();
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AttendanceHistoryPage));
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("OnCheckInClicked: Starting navigation to CheckInPage");
            await Shell.Current.GoToAsync(nameof(Pages.CheckInPage));
            System.Diagnostics.Debug.WriteLine("OnCheckInClicked: Navigation completed");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open check-in page: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"CheckIn navigation error: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void GotoCourseView(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Course course }) return;
        try
        {
            Shell.Current.GoToAsync(nameof(StudentCourseViewPage), new Dictionary<string, object>
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