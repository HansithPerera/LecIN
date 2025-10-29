using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;
using SupabaseShared.Models;
using Client = Supabase.Client;
using Exception = System.Exception;

namespace Lecin.PageModels.Student;

public partial class StudentViewProfilePageModel(Client client, AuthService _authService) : BasePageModel
{
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private SupabaseShared.Models.Student? _student;

    [ObservableProperty] private StudentFace? _studentFace;

    [RelayCommand]
    public async Task RemoveFaceAsync()
    {
        try
        {
            IsLoading = true;
            if (StudentFace == null) return;
            await client.Functions.Invoke("remove-student-face");
            StudentFace = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    public override async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            if (!Guid.TryParse(client.Auth.CurrentUser?.Id, out var userId)) return;
            Student = await client.From<SupabaseShared.Models.Student>()
                .Where(s => s.Id == userId)
                .Single();
            StudentFace = await client.From<StudentFace>()
                .Where(sf => sf.StudentId == userId)
                .Single();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
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