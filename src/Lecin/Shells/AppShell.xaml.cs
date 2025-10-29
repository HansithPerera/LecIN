using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;
using Lecin.Pages.Admin;
using Lecin.Pages.Student;
using Lecin.Pages.Teacher;
using Lecin.ViewModels;
using SupabaseShared.Models;
using Debug = System.Diagnostics.Debug;
using Font = Microsoft.Maui.Font;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;

namespace Lecin.Shells;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel vm)
    {
        BindingContext = vm;
        vm.AttendanceAlertReceived += OnAttendanceAlertReceived;
        InitializeComponent();
        RegisterRoutes();
        WeakReferenceMessenger.Default.Register<LoggedOutMessage>(this, (_, _) => GotoLogin());
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, msg) => NavigateToDashboard(msg.UserType));
    }

    public static string LoginPageRoute => nameof(LoginPage);

    public static string LandingPageRoute => nameof(LandingPage);

    public static string AdminDashboardPageRoute => nameof(AdminDashboardPage);

    public static string TeacherDashboardPageRoute => nameof(TeacherDashboardPage);

    public static string TeacherCourseListPageRoute => nameof(TeacherCourseListPage);

    public static string StudentDashboardPageRoute => nameof(StudentDashboardPage);

    public static string StudentProfilePageRoute => nameof(StudentProfilePage);

    public static string StudentViewProfilePageRoute => nameof(StudentViewProfilePage);

    public static string AttendanceHistoryPageRoute => nameof(AttendanceHistoryPage);

    public static string CheckInPageRoute => nameof(CheckInPage);
    
    public static string AdminListLocationPageRoute => nameof(AdminListLocationPage);
    
    public static string AdminListCameraPageRoute => nameof(AdminListCameraPage);

    public static string TeacherReportsPageRoute => nameof(TeacherReportsPage);

    /// <summary>
    ///     Update the theme segmented control based on the current app theme before the shell is visible.
    /// </summary>
    protected override void OnAppearing()
    {
        var theme = Preferences.Get("user_app_theme", "system");
        Application.Current!.UserAppTheme = theme switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => Application.Current!.UserAppTheme
        };
        ThemeSegmentedControl.SelectedIndex = Application.Current!.UserAppTheme switch
        {
            AppTheme.Light => 0,
            AppTheme.Dark => 1,
            _ => 2
        };
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(AttendanceHistoryPage), typeof(AttendanceHistoryPage));
        Routing.RegisterRoute(nameof(StudentProfilePage), typeof(StudentProfilePage));
        Routing.RegisterRoute(nameof(TeacherClassViewPage), typeof(TeacherClassViewPage));
        Routing.RegisterRoute(nameof(TeacherCourseViewPage), typeof(TeacherCourseViewPage));
        Routing.RegisterRoute(nameof(AttendanceStreaksPage), typeof(AttendanceStreaksPage));
        Routing.RegisterRoute(nameof(StudentCourseViewPage), typeof(StudentCourseViewPage));
        Routing.RegisterRoute(nameof(StudentRegisterFacePage), typeof(StudentRegisterFacePage));
        Routing.RegisterRoute(nameof(TeacherReportsPage), typeof(TeacherReportsPage));
        Routing.RegisterRoute(nameof(AdminViewCameraPage), typeof(AdminViewCameraPage));
        Routing.RegisterRoute(nameof(AdminViewLocationPage), typeof(AdminViewLocationPage));
        // Register CheckInPage as both a standard route and a root-level route
        Routing.RegisterRoute(nameof(CheckInPage), typeof(CheckInPage));
        Routing.RegisterRoute($"//{nameof(CheckInPage)}", typeof(CheckInPage));
    }
    
    private async void OnAttendanceAlertReceived(object? sender, Attendance e)
    {
        try
        {
            await DisplaySnackbarAsync(
                $"You checked into checked into your class successfully at {e.Timestamp.ToLocalTime():T}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to display attendance alert: " + ex.Message);
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

    /// <summary>
    ///     Handle theme changes from the segmented control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnThemeSegmentChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedIndex = e.NewIndex;
        Application.Current!.UserAppTheme = selectedIndex switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        var themePref = selectedIndex switch
        {
            0 => "light",
            1 => "dark",
            _ => "system"
        };
        Preferences.Set("user_app_theme", themePref);
    }

    private async void NavigateToDashboard(UserType userType)
    {
        try
        {
            switch (userType)
            {
                case UserType.Admin:
                    await Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
                    break;
                case UserType.Teacher:
                    await Current.GoToAsync($"//{nameof(TeacherCourseListPage)}");
                    break;
                case UserType.Student:
                    await Current.GoToAsync($"//{nameof(StudentDashboardPage)}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userType), userType, null);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to navigate to dashboard: " + e.Message);
        }
    }

    private async void GotoLogin()
    {
        try
        {
            await Task.Delay(1);
            await Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to navigate to login page: " + ex.Message);
        }
    }
}