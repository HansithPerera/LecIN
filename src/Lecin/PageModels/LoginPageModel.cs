using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;

namespace Lecin.PageModels;

public partial class LoginPageModel(AuthService authService) : BasePageModel
{
    [ObservableProperty] private string _email = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private UserType _userType = UserType.Student;

    public override Task LoadDataAsync()
    {
        Email = string.Empty;
        Password = string.Empty;
        StatusMessage = string.Empty;
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter email and password.";
            return;
        }

        try
        {
            var loginResult = await authService.SignIn(Email, Password, UserType);
            if (loginResult.IsOk)
            {
                StatusMessage = "Welcome!";
                WeakReferenceMessenger.Default.Send(new LoggedInMessage { UserType = UserType });
            }
            else
            {
                StatusMessage = loginResult.UnwrapErr() switch
                {
                    SignInError.InvalidCredentials => "Invalid email or password.",
                    SignInError.InvalidUserType => "User type does not match.",
                    _ => "An unknown error occurred."
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}