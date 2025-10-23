using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;
using SupabaseShared.Models;
using Client = Supabase.Client;

namespace Lecin.ViewModels;

public partial class AppShellViewModel : BasePageModel
{
    private readonly AuthService _authService;
    private readonly Client _client;

    private RealtimeChannel? _attendanceChannel;

    [ObservableProperty] private FlyoutBehavior _flyoutBehavior;

    [ObservableProperty] private bool _isAdmin;

    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] private bool _isStudent;

    [ObservableProperty] private bool _isTeacher;

    /// <inheritdoc />
    public AppShellViewModel(AuthService authService, Client client)
    {
        _authService = authService;
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, msg) => OnLoggedIn(msg.UserType));
        WeakReferenceMessenger.Default.Register<LoggedOutMessage>(this, (_, _) => OnLoggedOut());
        _client = client;
    }

    public event EventHandler<Attendance>? AttendanceAlertReceived;

    private void UnsubscribeFromAttendanceAlerts()
    {
        try
        {
            if (_attendanceChannel == null) return;
            _attendanceChannel.Unsubscribe();
            _attendanceChannel = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to unsubscribe from attendance alerts: {ex.Message}");
        }
    }

    private async void SubscribeToAttendanceAlerts()
    {
        try
        {
            await _client.Realtime.ConnectAsync();
            _attendanceChannel = _client.Realtime.Channel(table: "Attendance");

            _attendanceChannel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.Inserts,
                (_, change) =>
                {
                    var model = change.Model<Attendance>();
                    if (model == null) return;
                    AttendanceAlertReceived?.Invoke(this, model);
                });
            await _attendanceChannel.Subscribe();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to subscribe to attendance alerts: {ex.Message}");
        }
    }

    private void OnLoggedIn(UserType e)
    {
        ResetRoles();
        switch (e)
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
            default:
                throw new ArgumentOutOfRangeException();
        }

        IsLoggedIn = true;
        FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    private void OnLoggedOut()
    {
        ResetRoles();
        UnsubscribeFromAttendanceAlerts();
        IsLoggedIn = false;
        FlyoutBehavior = FlyoutBehavior.Disabled;
    }

    private void ResetRoles()
    {
        IsAdmin = false;
        IsStudent = false;
        IsTeacher = false;
    }

    [RelayCommand]
    private async Task Logout()
    {
        try
        {
            await _authService.SignOut();
            WeakReferenceMessenger.Default.Send(new LoggedOutMessage());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Logout failed: {ex.Message}");
        }
    }
}