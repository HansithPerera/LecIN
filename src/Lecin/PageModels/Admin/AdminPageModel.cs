using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Functions;
using SupabaseShared.Models;
using static System.Text.Encoding;

namespace Lecin.PageModels.Admin;

public partial class AdminPageModel : ObservableObject
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _isRefreshing;

    [ObservableProperty] ObservableCollection<Course> _courses = [];
    private readonly Supabase.Client _client;
    
    private readonly ModalErrorHandler _errorHandler;
    
    public AdminPageModel(Supabase.Client client, ModalErrorHandler errorHandler)
    {
        _client = client;
        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task DownloadAttendance(Course? course)
    {
        if (course == null) return;
        try
        {
            var options = new Client.InvokeFunctionOptions
            {
                Body = new Dictionary<string, object> { { "Code", course.Code }, { "SemesterCode", course.SemesterCode }, { "Year", course.Year }}
            };
            var text = await _client.Functions.Invoke<CsvResponse>("export-attendance-csv", options: options);
            if (text == null || string.IsNullOrWhiteSpace(text.csv))
            {
                _errorHandler.HandleError(new Exception("Failed to retrieve CSV data."));
            }
            var fileName = $"{course.Code}_Attendance_{course.Year}_S{course.SemesterCode}.csv";
            using var stream = new MemoryStream(UTF8.GetBytes(text.csv));
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
    }

    public async Task LoadDataAsync()
    {
        if (_dataLoaded && _isNavigatedTo) return;

        IsBusy = true;
        try
        {
            _dataLoaded = true;
            
            var courses = await _client.From<Course>()
                .Select("*")
                .Get();
            
            Courses = new ObservableCollection<Course>(courses.Models);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
}