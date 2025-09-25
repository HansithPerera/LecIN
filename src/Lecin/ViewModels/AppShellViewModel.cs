using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Lecin.ViewModels;

public class AppShellViewModel : INotifyPropertyChanged
{
    public bool IsAdmin { get; }
    public bool IsTeacher { get; }
    public bool IsStudent { get; }

    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            if (_isLoggedIn != value)
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand LogoutCommand { get; }

    public AppShellViewModel(List<string> roles, bool isLoggedIn = true)
    {
        IsAdmin = roles.Contains("Admin");
        IsTeacher = roles.Contains("Teacher");
        IsStudent = roles.Contains("Student");
        IsLoggedIn = isLoggedIn;

        LogoutCommand = new AsyncRelayCommand(OnLogoutAsync);
    }

    private async Task OnLogoutAsync()
    {
        // Clear token
        SecureStorage.Remove("jwt_token");

        // Go back to LoginPage
        Application.Current.MainPage = new NavigationPage(
            new Pages.LoginPage(new PageModels.LoginPageModel(App.CurrentSupabase))
        );

        // Mark as logged out
        IsLoggedIn = false;

        // Optional feedback
        await AppShell.DisplaySnackbarAsync("You have been logged out.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
