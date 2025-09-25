using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lecin.Models;
using Supabase;
using Supabase.Gotrue.Exceptions;

namespace Lecin.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly Client _supabase;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ICommand LoginCommand { get; }

    public LoginPageModel(Client supabase)
    {
        _supabase = supabase;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter email and password.";
            return;
        }

        try
        {
            var session = await _supabase.Auth.SignInWithPassword(Email, Password);
            if (session != null && session.User != null)
            {
                StatusMessage = $"Welcome, {session.User.Email}";
                await SecureStorage.Default.SetAsync("jwt_token", session.AccessToken);

                var userId = session.User.Id;
                var roles = new List<string>(); // ✅ collect all roles

                // ✅ Check Student role
                var studentResponse = await _supabase.From<Lecin.Models.Student>()
                .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Get();

                if (studentResponse.Models.FirstOrDefault() != null)
                    roles.Add("Student");

                // ✅ Check Teacher role
                var teacherResponse = await _supabase.From<Teacher>()
                    .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();
                if (teacherResponse.Models.FirstOrDefault() != null)
                    roles.Add("Teacher");

                // ✅ Check Admin role
                var adminResponse = await _supabase.From<Lecin.Models.Admin>()
                    .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();

                if (adminResponse.Models.FirstOrDefault() != null)
                    roles.Add("Admin");


                // ✅ Pass all roles + logged-in flag to AppShell
                // AppShell will decide which nav items to show
                Application.Current.MainPage = new AppShell(roles, isLoggedIn: true);

            }
            else
            {
                StatusMessage = "Login failed. Try again.";
            }
        }
        catch (GotrueException ex) when (ex.StatusCode == 400)
        {
            StatusMessage = "Invalid email or password.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"An error occurred: {ex.Message}";
        }
    }
}
