    using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
using Lecin.ViewModels;

namespace Lecin;

public partial class AppShell : Shell
{
    public AppShell(List<string> roles, bool isLoggedIn)   // ✅ roles list instead of single role
    {
        InitializeComponent();

        // Apply role-based visibility using ViewModel binding
        BindingContext = new AppShellViewModel(roles, isLoggedIn);


        // Initialize theme selector (Light/Dark)
        var currentTheme = Application.Current!.RequestedTheme;
        ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;

        // Register routes for navigation
        RegisterRoutes();
    }

    /// <summary>
    /// Register all navigation routes for Shell-based navigation.
    /// Any page you want to navigate to via GoToAsync() must be registered here.
    /// </summary>
    private void RegisterRoutes()
    {
        // Student subpages
        Routing.RegisterRoute(nameof(Pages.Student.AttendanceHistoryPage), typeof(Pages.Student.AttendanceHistoryPage));
        Routing.RegisterRoute(nameof(Pages.Student.AttendanceStreaksPage), typeof(Pages.Student.AttendanceStreaksPage));

        // Role dashboards
        Routing.RegisterRoute(nameof(Pages.AdminDashboardPage), typeof(Pages.AdminDashboardPage));
        Routing.RegisterRoute(nameof(Pages.TeacherDashboardPage), typeof(Pages.TeacherDashboardPage));
        Routing.RegisterRoute(nameof(Pages.StudentDashboardPage), typeof(Pages.StudentDashboardPage));
    }

    /// <summary>
    /// Display a snackbar message with custom styling.
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
    /// Display a toast message (not supported on Windows).
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
    /// Handle theme switch between Light/Dark when segmented control changes.
    /// </summary>
    private void SfSegmentedControl_SelectionChanged(object sender,
        Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // clean Supabase
        if (App.CurrentSupabase != null)
            await App.CurrentSupabase.Auth.SignOut();

        SecureStorage.Default.Remove("jwt_token");

        //LoginPage
        Application.Current.MainPage = new NavigationPage(
            new Pages.LoginPage(new PageModels.LoginPageModel(App.CurrentSupabase!))
        );

        await DisplayToastAsync("You have been logged out.");
    }





}
