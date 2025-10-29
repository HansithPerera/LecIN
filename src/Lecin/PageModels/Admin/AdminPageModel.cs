using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase;
using SupabaseShared.Models;
using static System.Text.Encoding;

namespace Lecin.PageModels.Admin;

public partial class AdminPageModel(Client client, ModalErrorHandler errorHandler) : BasePageModel
{
    [ObservableProperty] private ObservableCollection<Course> _courses = [];
    private bool _dataLoaded;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _isRefreshing;

    [RelayCommand]
    private async Task DownloadAttendance(Course? course)
    {
        if (course == null) return;
        try
        {
            var options = new Supabase.Functions.Client.InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                    { { "Code", course.Code }, { "SemesterCode", course.SemesterCode }, { "Year", course.Year } }
            };
            var text = await client.Functions.Invoke<CsvResponse>("export-attendance-csv", options: options);
            if (text == null || string.IsNullOrWhiteSpace(text.Csv))
                errorHandler.HandleError(new Exception("Failed to retrieve CSV data."));
            var fileName = $"{course.Code}_Attendance_{course.Year}_S{course.SemesterCode}.csv";
            using var stream = new MemoryStream(UTF8.GetBytes(text.Csv));
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }

    public override async Task LoadDataAsync()
    {
        if (_dataLoaded) return;

        IsBusy = true;
        try
        {
            _dataLoaded = true;

            var courses = await client.From<Course>()
                .Select("*")
                .Get();

            Courses = new ObservableCollection<Course>(courses.Models);
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}