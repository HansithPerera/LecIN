namespace Lecin.Pages.Student;

public partial class StudentDashboardPage : ContentPage
{
    public StudentDashboardPage()
    {
        InitializeComponent();
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AttendanceHistoryPage));
    }

    private async void OnStreaksClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AttendanceStreaksPage));
    }

    private async void OnViewAttendanceHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("attendancehistory");
    }

}