using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;
using SupabaseShared.Models;
using Client = Supabase.Client;

namespace Lecin.PageModels;


public partial class LoginPageModel(AuthService authService, Client client) : BasePageModel
{
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private UserType _userType = UserType.Student;

    
    [ObservableProperty] private string _maintenanceMessage = string.Empty;
    [ObservableProperty] private bool _showMaintenance;


    public override async Task LoadDataAsync()
    {
        
        Email = string.Empty;
        Password = string.Empty;
        StatusMessage = string.Empty;

        await base.LoadDataAsync();

        
        await client.InitializeAsync();

        try
        {
            
            var banner = await client
                .From<AppBanner>()
                .Where(b => b.IsActive == true)
                .Order(b => b.UpdatedAt!, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Single();

            if (banner is not null && banner.IsActive)
            {
                MaintenanceMessage = banner.Message;
                ShowMaintenance = true;
            }
            else
            {
                ShowMaintenance = false;
                MaintenanceMessage = string.Empty;
            }
        }
        catch
        {
            
            ShowMaintenance = false;
            MaintenanceMessage = string.Empty;
        }
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
                var err = loginResult.UnwrapErr();
                Debug.WriteLine($"Supabase login error: {err}");
                StatusMessage = err switch
                {
                    SignInError.InvalidCredentials => "Invalid email or password.",
                    SignInError.InvalidUserType => "User type does not match.",
                    _ => $"Sign in failed: {err}"
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
