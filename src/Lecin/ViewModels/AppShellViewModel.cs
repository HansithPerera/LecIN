using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lecin.Shells;
using Supabase;
using Supabase.Realtime.PostgresChanges;
using SupabaseShared.Models;

namespace Lecin.ViewModels;

public partial class AppShellViewModel : BasePageModel
{
    private readonly AuthService _authService;
    private readonly Client _client;

    [ObservableProperty] private FlyoutBehavior _flyoutBehavior;

    [ObservableProperty] private bool _isAdmin;

    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] private bool _isStudent;

    [ObservableProperty] private bool _isTeacher;

    public event EventHandler<Attendance>? AttendanceAlertReceived;
    
    public event EventHandler? LoggedOut;
    
    public event EventHandler<UserType>? LoggedIn;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ChangeThemeCommand))]
    private int _selectedIndex;

    /// <inheritdoc />
    public AppShellViewModel(AuthService authService, Client client)
    {
        _authService = authService;
        _client = client;
        _authService.LoggedIn += OnLoggedIn;
        _authService.LoggedOut += OnLoggedOut;
    }
    
    private async void SubscribeToAttendanceAlerts()
    {
        try
        {
            await _client.Realtime.ConnectAsync();
            var channel = _client.Realtime.Channel(table: "Attendance");
            
            channel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.Inserts, 
                (_, change) =>
                {
                    var model = change.Model<Attendance>();
                    if (model == null) return;
                    AttendanceAlertReceived?.Invoke(this, model);
                });
            await channel.Subscribe();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to subscribe to attendance alerts: {ex.Message}");
        }
    }

    private void OnLoggedIn(object? sender, LoggedInEventArgs e)
    {
        switch (e.UserType)
        {
            case UserType.Student:
                SubscribeToAttendanceAlerts();
                IsStudent = true;
                break;
            case UserType.Teacher:
                IsTeacher = true;
                break;
            case UserType.Admin:
                IsAdmin = true;
                break;
        }

        IsLoggedIn = true;
        SelectedIndex = Application.Current!.UserAppTheme == AppTheme.Light ? 0 : 1;
        FlyoutBehavior = FlyoutBehavior.Flyout;
        LoggedIn?.Invoke(this, e.UserType);
    }

    public void OnLoggedOut(object? sender, object e)
    {
        IsStudent = false;
        IsTeacher = false;
        IsAdmin = false;
        IsLoggedIn = false;
        FlyoutBehavior = FlyoutBehavior.Disabled;
    }

    [RelayCommand]
    private Task ChangeTheme()
    {
        var theme = SelectedIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        Application.Current!.UserAppTheme = theme;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Logout()
    {
        try
        {
            await _authService.SignOut();
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Logout failed: {ex.Message}");
        }
    }
}