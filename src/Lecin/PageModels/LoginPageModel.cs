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
                string role = "Unknown";

                // Student
                var studentResponse = await _supabase.From<Student>()
                    .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();
                if (student != null)
                {
                    role = "Student";
                }

                // Teacher
                var teacherResponse = await _supabase.From<Teacher>()
                    .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();

                var teacher = teacherResponse.Models.FirstOrDefault();
                if (teacher != null)
                {
                    role = "Teacher";
                }

                // Admin
                var adminResponse = await _supabase.From<Admin>()
                    .Filter("Id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Get();

                var admin = adminResponse.Models.FirstOrDefault();
                if (admin != null)
                {
                    role = "Admin";
                }


                // Load role-specific AppShell
                Application.Current.MainPage = new AppShell(role);
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
