using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Lecin.ViewModels;
using SupabaseShared.Models;
using Debug = System.Diagnostics.Debug;
using Font = Microsoft.Maui.Font;

namespace Lecin.Shells;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
        vm.LoggedIn += OnLoggedIn;
        vm.LoggedOut += OnLoggedOut;
        vm.AttendanceAlertReceived += OnAttendanceAlertReceived;
    }

    private async void OnAttendanceAlertReceived(object? sender, Attendance e)
    {
        try
        {
            await DisplaySnackbarAsync($"You checked into checked into your class successfully at {e.Timestamp.ToLocalTime():T}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to display attendance alert: " + ex.Message);
        }
    }

    private void OnLoggedOut(object? sender, EventArgs _)
    {
        try
        {
            Current.GoToAsync("login");
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to navigate to login page: " + e.Message);
        }
    }

    private void OnLoggedIn(object? sender, UserType userType)
    {
        try
        {
            switch (userType)
            {
                case UserType.Admin:
                    Current.GoToAsync("admin/dashboard");
                    break;
                case UserType.Teacher:
                    Current.GoToAsync("teacher/dashboard");
                    break;
                case UserType.Student:
                    Current.GoToAsync("student/dashboard");
                    break;
                default:
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to navigate to dashboard: " + e.Message);
        }
    }

    /// <summary>
    ///     Display a snackbar message with custom styling.
    /// </summary>
    public static async Task DisplaySnackbarAsync(string message)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = Color.FromArgb("#FF3300"),
            TextColor = Colors.White,
            ActionButtonTextColor = Colors.Yellow,
            CornerRadius = new CornerRadius(0),
            Font = Font.SystemFontOfSize(18),
            ActionButtonFont = Font.SystemFontOfSize(14)
        };

        var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

        await snackbar.Show(cancellationTokenSource.Token);
    }

    /// <summary>
    ///     Display a toast message (not supported on Windows).
    /// </summary>
    public static async Task DisplayToastAsync(string message)
    {
        if (OperatingSystem.IsWindows())
            return;

        var toast = Toast.Make(message, textSize: 18);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await toast.Show(cts.Token);
    }
}