using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lecin.PageModels;

public partial class LoginPageModel(AuthService authService) : BasePageModel
{
    [ObservableProperty] private string _email = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private UserType _userType = UserType.Student;

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
                StatusMessage = "Welcome!";
            else
                StatusMessage = loginResult.UnwrapErr() switch
                {
                    SignInError.InvalidCredentials => "Invalid email or password.",
                    SignInError.InvalidUserType => "User type does not match.",
                    _ => "An unknown error occurred."
                };
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}